// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using DbUp;
using DbUp.Builder;

using Gnomeshade.Data.Migrations;

using Microsoft.Extensions.Logging;

using Npgsql;

namespace Gnomeshade.Data.PostgreSQL.Migrations;

internal sealed class PostgreSQLDatabaseMigrator : DatabaseMigrator<NpgsqlConnection>
{
	public PostgreSQLDatabaseMigrator(ILogger<PostgreSQLDatabaseMigrator> logger, NpgsqlConnection connection)
		: base(logger, connection)
	{
	}

	/// <inheritdoc />
	protected override void CheckDatabase(SupportedDatabasesForEnsureDatabase supportedDatabases)
	{
		supportedDatabases.PostgresqlDatabase(Connection.Settings.ToString(), UpgradeLog);
	}

	/// <inheritdoc />
	protected override UpgradeEngineBuilder GetBuilder(SupportedDatabases supportedDatabases)
	{
		return supportedDatabases.PostgresqlDatabase(Connection.Settings.ToString());
	}

	/// <inheritdoc />
	protected override bool ScriptFilter(string filepath)
	{
		return base.ScriptFilter(filepath)
			|| filepath.Contains("Gnomeshade.Data.Migrations", StringComparison.OrdinalIgnoreCase);
	}
}
