// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Reflection;

using DbUp;
using DbUp.Builder;

using Gnomeshade.Data.Migrations;

using Microsoft.Extensions.Logging;

using Npgsql;

namespace Gnomeshade.Data.PostgreSQL.Migrations;

internal sealed class PostgreSQLDatabaseMigrator : DatabaseMigrator<NpgsqlConnection>
{
	private readonly NpgsqlDataSource _dataSource;

	public PostgreSQLDatabaseMigrator(ILogger<PostgreSQLDatabaseMigrator> logger, NpgsqlDataSource dataSource)
		: base(logger)
	{
		_dataSource = dataSource;
	}

	/// <inheritdoc />
	protected override void CheckDatabase(SupportedDatabasesForEnsureDatabase supportedDatabases)
	{
		var connectionStringBuilder = GetConnectionStringBuilder();
		supportedDatabases.PostgresqlDatabase(connectionStringBuilder.ToString(), UpgradeLog);
	}

	/// <inheritdoc />
	protected override UpgradeEngineBuilder GetBuilder(SupportedDatabases supportedDatabases)
	{
		var connectionStringBuilder = GetConnectionStringBuilder();
		return supportedDatabases.PostgresqlDatabase(connectionStringBuilder.ToString());
	}

	/// <inheritdoc />
	protected override bool ScriptFilter(string filepath)
	{
		return base.ScriptFilter(filepath) ||
			filepath.Contains("Gnomeshade.Data.Migrations", StringComparison.OrdinalIgnoreCase);
	}

	private NpgsqlConnectionStringBuilder GetConnectionStringBuilder()
	{
		var property = typeof(NpgsqlDataSource)
				.GetProperty("Settings", BindingFlags.Instance | BindingFlags.NonPublic) ??
			throw new MissingMemberException(typeof(NpgsqlDataSource).FullName, "Settings");

		return (NpgsqlConnectionStringBuilder)property.GetValue(_dataSource)!;
	}
}
