// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Gnomeshade.WebApi.Configuration.Options;

/// <summary>Options for configuring Open Telemetry.</summary>
public sealed class OpenTelemetryOptions
{
	internal const string SectionName = "OpenTelemetry";

	/// <summary>Gets a value indicating whether to enable Open Telemetry.</summary>
	public bool Enabled { get; init; } = true;

	/// <summary>Gets the name of the service.</summary>
	[Required]
	public string ServiceName { get; init; } = "Gnomeshade";

	/// <summary>Gets the version of the service. Defaults to the assembly version.</summary>
	[Required]
	public string ServiceVersion { get; init; } = Assembly.GetExecutingAssembly().GetName().Version?.ToString()!;

	/// <summary>Gets the endpoint to which to send the telemetry. Defaults to localhost.</summary>
	[Required]
	public Uri ExporterEndpoint { get; init; } = new("http://localhost:4317");
}
