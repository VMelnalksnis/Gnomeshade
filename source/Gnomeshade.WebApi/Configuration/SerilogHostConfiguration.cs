// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

using Elastic.Apm.SerilogEnricher;
using Elastic.CommonSchema.Serilog;

using Elasticsearch.Net;

using Gnomeshade.WebApi.Configuration.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using Serilog;
using Serilog.Events;
using Serilog.Extensions.Hosting;
using Serilog.Sinks.Elasticsearch;

namespace Gnomeshade.WebApi.Configuration;

internal static class SerilogHostConfiguration
{
	internal static ReloadableLogger CreateBoostrapLogger() => new LoggerConfiguration()
		.Enrich.FromLogContext()
		.WriteTo.Console()
		.MinimumLevel.Verbose()
		.CreateBootstrapLogger();

	[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "The type is primitive")]
	internal static void Configure(HostBuilderContext context, LoggerConfiguration configuration)
	{
		configuration
			.ReadFrom.Configuration(context.Configuration)
			.Enrich.FromLogContext()
			.Enrich.WithElasticApmCorrelationInfo()
			.WriteTo.Console();

		if (!context.Configuration.GetValidIfDefined<ElasticSearchLoggingOptions>(out var options))
		{
			return;
		}

		Log.Debug("Configuring Elasticsearch logging");

		var ecsConfiguration = new EcsTextFormatterConfiguration();
		if (context.Configuration
				.GetChildren()
				.SingleOrDefault(section => section.Key == "ElasticApm")?
				.GetChildren()
				.Any(section => section.Key == "ServiceName") ??
			false)
		{
			Log.Debug("Configuring ECS formatter service name");

			(ecsConfiguration as IEcsTextFormatterConfiguration).MapCustom = (@base, _) =>
			{
				var serviceName = context.Configuration.GetValue<string>("ElasticApm:ServiceName");
				@base.Service ??= new();
				@base.Service.Name = serviceName;
				return @base;
			};
		}

		Log.Debug("Adding Elasticsearch sink");
		configuration
			.WriteTo.Elasticsearch(new(options.Nodes)
			{
				AutoRegisterTemplate = false,
				AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
				CustomFormatter = new EcsTextFormatter(ecsConfiguration),
				EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog,
				MinimumLogEventLevel = LogEventLevel.Information,
				TypeName = null, // _type is removed in ElasticSearch 8.x.x
				InlineFields = true,
				IndexFormat = "logs-gnomeshade-{0:yyyy.MM.dd}",
				BatchAction = ElasticOpType.Create,
				ModifyConnectionSettings = connection =>
				{
					Log.Debug("Modifying Elasticsearch connection settings");

					connection.BasicAuthentication(options.Username, options.Password);
					connection.MemoryStreamFactory(RecyclableMemoryStreamFactory.Default);
					if (options.CertificateFilePath is null)
					{
						return connection;
					}

					Log.Debug("Configuring Elasticsearch connection certificates");
					var clientCertificate = X509Certificate2.CreateFromPemFile(options.CertificateFilePath, options.KeyFilePath);
					connection.ClientCertificates(new(new X509Certificate[] { clientCertificate }));
					connection.ServerCertificateValidationCallback((_, certificate, chain, _) =>
					{
						Log.Debug(
							"Validating ElasticSearch server certificate for {Subject} issued by {Issuer}",
							certificate.Subject,
							certificate.Issuer);

						// From https://stackoverflow.com/a/51137681, todo - test/validate this #172
						try
						{
							if (options.CertificateAuthorityFilePath is not null)
							{
								var authorityCertificate = X509Certificate.CreateFromCertFile(options.CertificateAuthorityFilePath!);
								var certificateAuthority = new X509Certificate2(authorityCertificate);

								// Now that we have tested to see if the cert builds properly, we now will check if the thumbprint of the root ca matches our trusted one
								if (chain.ChainElements[^1].Certificate.Thumbprint != certificateAuthority.Thumbprint)
								{
									return false;
								}

								// Once we have verified the thumbprint the last fun check we can do is to build the chain and then see if the remote cert builds properly with it
								// Testing to see if the Certificate and Chain build properly, aka no forgery.
								var trustedChain = new X509Chain();
								trustedChain.ChainPolicy.ExtraStore.Add(certificateAuthority);
								trustedChain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
								trustedChain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
								trustedChain.Build(new(certificate));

								// Looking to see if there are no errors in the build that we don’t like
								foreach (var status in trustedChain.ChainStatus)
								{
									if (status.Status is X509ChainStatusFlags.NoError or X509ChainStatusFlags.UntrustedRoot)
									{
										// Acceptable Status, We want to know if it builds properly.
									}
									else
									{
										return false;
									}
								}
							}
							else
							{
								chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
								chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
								chain.Build(new(certificate));

								// Looking to see if there are no errors in the build that we don’t like
								foreach (var status in chain.ChainStatus)
								{
									if (status.Status is X509ChainStatusFlags.NoError or X509ChainStatusFlags.UntrustedRoot)
									{
										// Acceptable Status, We want to know if it builds properly.
									}
									else
									{
										return false;
									}
								}
							}
						}
						catch (Exception exception)
						{
							Log.Error(exception, "Failed to validate ElasticSearch server certificate");
							return false;
						}

						return true;
					});

					Log.Debug("Finished modifying Elasticsearch connection settings");
					return connection;
				},
			});

		Log.Debug("Finished configuring Elasticsearch logging");
	}
}
