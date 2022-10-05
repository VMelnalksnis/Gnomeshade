// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Configuration;
using Gnomeshade.WebApi.Configuration.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Gnomeshade.WebApi.HealthChecks;

/// <summary>Checks that all OIDC providers configured in <see cref="OidcProviderOptions.OidcProviderSectionName"/> section.</summary>
public sealed class OidcHealthCheck : IHealthCheck
{
	private readonly ILogger<OidcHealthCheck> _logger;
	private readonly IConfiguration _configuration;
	private readonly HttpClient _httpClient;

	/// <summary>Initializes a new instance of the <see cref="OidcHealthCheck"/> class.</summary>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="configuration">Application configuration.</param>
	/// <param name="httpClient">HTTP client for checking the OIDC providers.</param>
	public OidcHealthCheck(ILogger<OidcHealthCheck> logger, IConfiguration configuration, HttpClient httpClient)
	{
		_logger = logger;
		_configuration = configuration;
		_httpClient = httpClient;
	}

	/// <inheritdoc />
	public async Task<HealthCheckResult> CheckHealthAsync(
		HealthCheckContext context,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var providerSection = _configuration.GetSection(OidcProviderOptions.OidcProviderSectionName);
			var providerNames = providerSection.GetChildren().Select(section => section.Key);
			foreach (var providerName in providerNames)
			{
				var healthCheckResult = await CheckOidcProvider(providerSection, providerName, cancellationToken);
				if (healthCheckResult is not null)
				{
					return healthCheckResult.Value;
				}
			}

			return HealthCheckResult.Healthy("Successfully got metadata from all OIDC providers");
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Failed OIDC provider health check");
			return HealthCheckResult.Unhealthy("Failed to check OIDC providers");
		}
	}

	private async Task<HealthCheckResult?> CheckOidcProvider(
		IConfiguration providerSection,
		string providerName,
		CancellationToken cancellationToken)
	{
		var keycloakOptions = providerSection.GetValid<OidcProviderOptions>(providerName);
		var response = await _httpClient.GetAsync(keycloakOptions.Metadata, cancellationToken);
		if (response.IsSuccessStatusCode)
		{
			return null;
		}

		var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
		_logger.LogError(
			"Failed to get metadata for {OidcProviderName} - {OidcMetadataResponse}",
			providerName,
			responseContent);

		return HealthCheckResult.Unhealthy($"Failed to get metadata for {providerName}");
	}
}
