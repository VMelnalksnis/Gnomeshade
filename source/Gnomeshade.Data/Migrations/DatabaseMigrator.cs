// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data.Common;

using DbUp;
using DbUp.Builder;
using DbUp.Engine.Output;

using Microsoft.Extensions.Logging;

namespace Gnomeshade.Data.Migrations;

/// <inheritdoc />
public abstract class DatabaseMigrator<TConnection> : IDatabaseMigrator
	where TConnection : DbConnection
{
	private readonly ILogger<DatabaseMigrator<TConnection>> _logger;

	/// <summary>Initializes a new instance of the <see cref="DatabaseMigrator{TConnection}"/> class.</summary>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="connection">A connection to the database to which to apply migrations to.</param>
	protected DatabaseMigrator(ILogger<DatabaseMigrator<TConnection>> logger, TConnection connection)
	{
		Connection = connection;
		_logger = logger;

		UpgradeLog = new DatabaseUpgradeLogger<DatabaseMigrator<TConnection>>(logger);
	}

	/// <summary>Gets the database migration logger.</summary>
	protected IUpgradeLog UpgradeLog { get; }

	/// <summary>Gets a connection to the database to which to apply migrations to.</summary>
	protected TConnection Connection { get; }

	/// <inheritdoc />
	public void Migrate()
	{
		CheckDatabase(EnsureDatabase.For);

		var upgradeEngine = GetBuilder(DeployChanges.To)
			.WithScriptsEmbeddedInAssembly(GetType().Assembly, ScriptFilter)
			.WithTransaction()
			.LogTo(UpgradeLog)
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

	/// <summary>Checks that the specific database is accessible.</summary>
	/// <param name="supportedDatabases">Databases that have migration support.</param>
	protected abstract void CheckDatabase(SupportedDatabasesForEnsureDatabase supportedDatabases);

	/// <summary>Gets the upgrade engine builder for the specific database from <paramref name="supportedDatabases"/>.</summary>
	/// <param name="supportedDatabases">Databases that have migration support.</param>
	/// <returns>Builder for the specific database.</returns>
	protected abstract UpgradeEngineBuilder GetBuilder(SupportedDatabases supportedDatabases);

	/// <summary>Filters migration scripts that need to be run.</summary>
	/// <param name="filepath">The filepath of the script.</param>
	/// <returns>Whether to run the specified migration scripts.</returns>
	/// <exception cref="InvalidOperationException">The current type does not have a namespace.</exception>
	protected virtual bool ScriptFilter(string filepath)
	{
		var migrationNamespace = GetType().Namespace;
		if (migrationNamespace is null)
		{
			throw new InvalidOperationException($"{GetType().FullName} does not have a namespace");
		}

		return filepath.Contains(migrationNamespace, StringComparison.OrdinalIgnoreCase);
	}
}
