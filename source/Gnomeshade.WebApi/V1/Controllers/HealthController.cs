// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.WebApi.V1.Controllers;

/// <summary>Check the current status of the API.</summary>
[AllowAnonymous]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class HealthController : ControllerBase
{
	private static readonly string _unhealthy = HealthStatus.Unhealthy.ToString();
	private static readonly string _degraded = HealthStatus.Degraded.ToString();
	private static readonly string _healthy = HealthStatus.Healthy.ToString();

	private readonly HealthCheckService _healthCheckService;

	/// <summary>Initializes a new instance of the <see cref="HealthController"/> class.</summary>
	/// <param name="healthCheckService">Service for checking the health of the API.</param>
	public HealthController(HealthCheckService healthCheckService)
	{
		_healthCheckService = healthCheckService;
	}

	/// <summary>Gets the current status of the API.</summary>
	/// <returns>The current status of the API.</returns>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <response code="200">The API is either fully operational or degraded.</response>
	/// <response code="503">The API is not operational.</response>
	[HttpGet]
	[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
	[ProducesResponseType<string>(Status200OK)]
	[ProducesResponseType<string>(Status503ServiceUnavailable)]
	public async Task<ActionResult<string>> Get(CancellationToken cancellationToken)
	{
		var report = await _healthCheckService.CheckHealthAsync(cancellationToken);
		return report.Status switch
		{
			HealthStatus.Unhealthy => StatusCode(Status503ServiceUnavailable, _unhealthy),
			HealthStatus.Degraded => Ok(_degraded),
			HealthStatus.Healthy => Ok(_healthy),
			_ => throw new ArgumentOutOfRangeException(nameof(report.Status), report.Status, "Unexpected health report status"),
		};
	}
}
