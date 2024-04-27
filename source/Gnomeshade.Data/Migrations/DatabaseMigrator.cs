// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data.Common;
using System.Linq;

using DbUp;
using DbUp.Builder;
using DbUp.Engine;
using DbUp.Engine.Output;

using Microsoft.Extensions.Logging;

using static Microsoft.Extensions.Logging.LogLevel;

namespace Gnomeshade.Data.Migrations;

/// <inheritdoc />
public abstract partial class DatabaseMigrator<TConnection> : IDatabaseMigrator
	where TConnection : DbConnection
{
	private readonly ILogger<DatabaseMigrator<TConnection>> _logger;
	private readonly IScriptPreprocessor? _scriptPreprocessor;

	/// <summary>Initializes a new instance of the <see cref="DatabaseMigrator{TConnection}"/> class.</summary>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="connection">A connection to the database to which to apply migrations to.</param>
	/// <param name="scriptPreprocessor">An optional script preprocessor.</param>
	protected DatabaseMigrator(
		ILogger<DatabaseMigrator<TConnection>> logger,
		TConnection connection,
		IScriptPreprocessor? scriptPreprocessor = null)
	{
		Connection = connection;
		_scriptPreprocessor = scriptPreprocessor;
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

		var migrationScriptAssemblies = new[] { typeof(DatabaseMigrator<>).Assembly, GetType().Assembly };

		var builder = GetBuilder(DeployChanges.To)
			.WithScriptsEmbeddedInAssemblies(migrationScriptAssemblies, ScriptFilter)
			.WithScriptNameComparer(new MigrationScriptNameComparer())
			.WithTransaction()
			.LogScriptOutput()
			.LogTo(UpgradeLog);

		if (_scriptPreprocessor is { } preprocessor)
		{
			builder = builder.WithPreprocessor(preprocessor);
		}

		var upgradeEngine = builder.Build();
		if (!upgradeEngine.IsUpgradeRequired())
		{
			MigrationNotRequired();
			return;
		}

		MigrationRequired();
		var upgradeResult = upgradeEngine.PerformUpgrade();

		if (upgradeResult.Successful)
		{
			MigrationSuccessful();
			return;
		}

		MigrationFailed(upgradeResult.Error, upgradeResult.ErrorScript.Name);
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
		var namespaces = new[] { typeof(DatabaseMigrator<>).Namespace, GetType().Namespace }
			.Where(name => name is not null)
			.Select(name => name!);

		return namespaces.Any(name => filepath.Contains(name, StringComparison.OrdinalIgnoreCase));
	}

	[LoggerMessage(EventId = 1, Level = Information, Message = "Database migration is not required")]
	private partial void MigrationNotRequired();

	[LoggerMessage(EventId = 2, Level = Information, Message = "Starting database migration")]
	private partial void MigrationRequired();

	[LoggerMessage(EventId = 3, Level = Information, Message = "Database migration completed successfully")]
	private partial void MigrationSuccessful();

	[LoggerMessage(EventId = 4, Level = Critical, Message = "Database migration failed at script {MigrationScript}")]
	private partial void MigrationFailed(Exception exception, string migrationScript);
}
