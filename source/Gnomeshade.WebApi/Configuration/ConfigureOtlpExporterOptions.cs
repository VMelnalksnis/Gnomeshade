// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Gnomeshade.WebApi.Configuration.Options;

using Microsoft.Extensions.Options;

using OpenTelemetry.Exporter;

namespace Gnomeshade.WebApi.Configuration;

internal sealed class ConfigureOtlpExporterOptions : IConfigureOptions<OtlpExporterOptions>
{
	private readonly IOptionsMonitor<OpenTelemetryOptions> _optionsMonitor;

	public ConfigureOtlpExporterOptions(IOptionsMonitor<OpenTelemetryOptions> optionsMonitor)
	{
		_optionsMonitor = optionsMonitor;
	}

	/// <inheritdoc />
	public void Configure(OtlpExporterOptions options)
	{
		var telemetryOptions = _optionsMonitor.CurrentValue;

		options.Endpoint = telemetryOptions.ExporterEndpoint;
		options.Protocol = OtlpExportProtocol.Grpc;
	}
}
