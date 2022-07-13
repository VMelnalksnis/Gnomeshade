// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

using VMelnalksnis.NordigenDotNet;

namespace Gnomeshade.Interfaces.WebApi.HealthChecks;

/// <summary>Checks that Nordigen API is accessible.</summary>
public sealed class NordigenHealthCheck : IHealthCheck
{
	private const string _sandboxInstitutionId = "SANDBOXFINANCE_SFIN0000";

	private readonly ILogger<NordigenHealthCheck> _logger;
	private readonly INordigenClient _nordigenClient;

	/// <summary>Initializes a new instance of the <see cref="NordigenHealthCheck"/> class.</summary>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="nordigenClient">Nordigen API client.</param>
	public NordigenHealthCheck(ILogger<NordigenHealthCheck> logger, INordigenClient nordigenClient)
	{
		_logger = logger;
		_nordigenClient = nordigenClient;
	}

	/// <inheritdoc />
	public async Task<HealthCheckResult> CheckHealthAsync(
		HealthCheckContext context,
		CancellationToken cancellationToken = default)
	{
		try
		{
			_ = await _nordigenClient.Institutions.Get(_sandboxInstitutionId, cancellationToken);
			return HealthCheckResult.Healthy("Successfully retrieved data from Nordigen");
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Failed Nordigen health check");
			return HealthCheckResult.Unhealthy("Failed to retrieve data from Nordigen");
		}
	}
}
