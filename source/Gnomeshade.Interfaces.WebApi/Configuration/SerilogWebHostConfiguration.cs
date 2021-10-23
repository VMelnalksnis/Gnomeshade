// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Elastic.Apm.SerilogEnricher;
using Elastic.CommonSchema.Serilog;

using Elasticsearch.Net;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

using Serilog;
using Serilog.Events;
using Serilog.Extensions.Hosting;
using Serilog.Sinks.Elasticsearch;

namespace Gnomeshade.Interfaces.WebApi.Configuration
{
	internal static class SerilogWebHostConfiguration
	{
		internal static ReloadableLogger CreateBoostrapLogger()
		{
			return
				new LoggerConfiguration()
					.Enrich.FromLogContext()
					.WriteTo.Console()
					.MinimumLevel.Verbose()
					.CreateBootstrapLogger();
		}

		internal static void Configure(WebHostBuilderContext context, LoggerConfiguration configuration)
		{
			var options = new ElasticSearchLoggingOptions();
			context.Configuration.Bind(ElasticSearchLoggingOptions.SectionName, options);

			configuration
				.Enrich.FromLogContext()
				.Enrich.WithElasticApmCorrelationInfo()
				.WriteTo.Console()
				.WriteTo.Elasticsearch(new(options.Nodes)
				{
					AutoRegisterTemplate = true,
					AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
					CustomFormatter = new EcsTextFormatter(),
					EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog,
					MinimumLogEventLevel = LogEventLevel.Information,
					ModifyConnectionSettings = connection =>
					{
						// connection.ClientCertificates(new X509Certificate2Collection()); todo
						connection.ServerCertificateValidationCallback((_, _, _, _) => true); // todo
						connection.BasicAuthentication(options.Username, options.Password);
						connection.MemoryStreamFactory(RecyclableMemoryStreamFactory.Default);
						return connection;
					},
				});
		}
	}
}
