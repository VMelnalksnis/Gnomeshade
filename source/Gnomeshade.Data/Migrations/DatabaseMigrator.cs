// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using DbUp;
using DbUp.Engine.Output;

using Microsoft.Extensions.Logging;

namespace Gnomeshade.Data.Migrations;

/// <summary>Migrates database to the latest version.</summary>
public sealed class DatabaseMigrator
{
	private readonly ILogger<DatabaseMigrator> _logger;
	private readonly IUpgradeLog _upgradeLog;

	/// <summary>Initializes a new instance of the <see cref="DatabaseMigrator"/> class.</summary>
	/// <param name="logger">Logger for logging in the specified category.</param>
	public DatabaseMigrator(ILogger<DatabaseMigrator> logger)
	{
		_logger = logger;

		_upgradeLog = new DatabaseUpgradeLogger<DatabaseMigrator>(logger);
	}

	/// <summary>Ensures that the database is created and upgraded to the latest version.</summary>
	/// <param name="connectionString">The connection string of the database to migrate.</param>
	public void Migrate(string connectionString)
	{
		EnsureDatabase.For.PostgresqlDatabase(connectionString, _upgradeLog);

		var upgradeEngine = DeployChanges.To
			.PostgresqlDatabase(connectionString)
			.WithScriptsEmbeddedInAssembly(typeof(DatabaseMigrator).Assembly, ScriptFilter)
			.WithTransaction()
			.LogTo(_upgradeLog)
			.Build();

		if (!upgradeEngine.IsUpgradeRequired())
		{
			_logger.LogInformation("Database upgrade is not required");
			return;
		}

		_logger.LogInformation("Database upgrade is required, starting now");
		var upgradeResult = upgradeEngine.PerformUpgrade();

		if (upgradeResult.Successful)
		{
			_logger.LogInformation("Database upgrade completed successfully");
			return;
		}

		_logger.LogError(
			upgradeResult.Error,
			"Database upgrade failed at script {MigrationScript}",
			upgradeResult.ErrorScript.Name);

		throw upgradeResult.Error;
	}

	private static bool ScriptFilter(string filepath)
	{
		var migrationNamespace = typeof(DatabaseMigrator).Namespace;
		if (migrationNamespace is null)
		{
			throw new InvalidOperationException($"{typeof(DatabaseMigrator).FullName} does not have a namespace");
		}

		return filepath.Contains(migrationNamespace, StringComparison.OrdinalIgnoreCase);
	}
}
