// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Gnomeshade.WebApi.HealthChecks;

/// <summary>Checks that the database used by the application is accessible.</summary>
public sealed class DatabaseHealthCheck : IHealthCheck
{
	private readonly ILogger<DatabaseHealthCheck> _logger;
	private readonly IDbConnection _dbConnection;

	/// <summary>Initializes a new instance of the <see cref="DatabaseHealthCheck"/> class.</summary>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="dbConnection">The database connection to check.</param>
	public DatabaseHealthCheck(ILogger<DatabaseHealthCheck> logger, IDbConnection dbConnection)
	{
		_logger = logger;
		_dbConnection = dbConnection;
	}

	/// <inheritdoc />
	public Task<HealthCheckResult> CheckHealthAsync(
		HealthCheckContext context,
		CancellationToken cancellationToken = default)
	{
		try
		{
			_dbConnection.Open();
			_dbConnection.Close();
			return Task.FromResult(HealthCheckResult.Healthy("Successfully connected to the database"));
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Failed database health check");
			return Task.FromResult(HealthCheckResult.Unhealthy("Failed to connect to the database"));
		}
	}
}
