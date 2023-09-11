// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

using Gnomeshade.WebApi.Configuration.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Npgsql;

using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Gnomeshade.WebApi.Configuration;

internal static class OpenTelemetryExtensions
{
	internal static IServiceCollection AddGnomeshadeOpenTelemetry(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		services.AddValidatedOptions<OpenTelemetryOptions>(configuration);

		var telemetryOptions = configuration.GetValid<OpenTelemetryOptions>();
		if (!telemetryOptions.Enabled)
		{
			return services;
		}

		var serviceName = telemetryOptions.ServiceName;
		var serviceVersion = telemetryOptions.ServiceVersion;
		var environment = configuration.GetValue<string>("environment") ?? "Development";

		var resourceBuilder = ResourceBuilder
			.CreateEmpty()
			.AddTelemetrySdk()
			.AddService(serviceName, serviceVersion: serviceVersion)
			.AddAttributes(new Dictionary<string, object> { { "deployment.environment", environment } });

		services
			.AddSingleton<IConfigureOptions<OtlpExporterOptions>, ConfigureOtlpExporterOptions>()
			.AddOpenTelemetry()
			.WithTracing(builder =>
			{
				builder.AddOtlpExporter();
				builder.SetResourceBuilder(resourceBuilder);
				builder.AddSource(serviceName);

				builder
					.AddHttpClientInstrumentation(options => options.RecordException = true)
					.AddAspNetCoreInstrumentation(options => options.RecordException = true)
					.AddNpgsql();
			})
			.WithMetrics(builder =>
			{
				builder.AddOtlpExporter();
				builder.SetResourceBuilder(resourceBuilder);

				builder
					.AddProcessInstrumentation()
					.AddHttpClientInstrumentation()
					.AddAspNetCoreInstrumentation()
					.AddRuntimeInstrumentation();
			});

		services.AddLogging(builder => builder.AddOpenTelemetry(options =>
		{
			options.AddOtlpExporter((exporterOptions, loggerOptions) =>
			{
				loggerOptions.ExportProcessorType = ExportProcessorType.Simple;

				// HACK: Force the open telemetry endpoint environment variable to be populated even if
				// the value is already provided through other configuration means.
				//
				// https://github.com/open-telemetry/opentelemetry-dotnet/issues/4014

				var defaultOptions = new OpenTelemetryOptions();
				var endpoint = configuration.GetValue<Uri>($"{OpenTelemetryOptions.SectionName}:{nameof(OpenTelemetryOptions.ExporterEndpoint)}") ?? defaultOptions.ExporterEndpoint;
				var protocol = configuration.GetValue<OtlpExportProtocol>($"{OpenTelemetryOptions.SectionName}:{nameof(OpenTelemetryOptions.ExportProtocol)}");

				exporterOptions.Endpoint = endpoint;
				exporterOptions.Protocol = protocol;
			});
			options.SetResourceBuilder(resourceBuilder);
			options.IncludeScopes = true;
			options.IncludeFormattedMessage = true;
			options.ParseStateValues = true;
		}));

		return services;
	}
}
