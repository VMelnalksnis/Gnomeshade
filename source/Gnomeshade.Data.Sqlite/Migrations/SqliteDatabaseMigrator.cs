// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using DbUp;
using DbUp.Builder;
using DbUp.SQLite.Helpers;

using Gnomeshade.Data.Migrations;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace Gnomeshade.Data.Sqlite.Migrations;

internal sealed class SqliteDatabaseMigrator : DatabaseMigrator<SqliteConnection>
{
	private readonly SqliteConnection _connection;

	public SqliteDatabaseMigrator(ILogger<SqliteDatabaseMigrator> logger, SqliteConnection connection)
		: base(logger)
	{
		_connection = connection;
	}

	/// <inheritdoc />
	protected override void CheckDatabase(SupportedDatabasesForEnsureDatabase supportedDatabases)
	{
	}

	/// <inheritdoc />
	protected override UpgradeEngineBuilder GetBuilder(SupportedDatabases supportedDatabases)
	{
		return supportedDatabases.SQLiteDatabase(new SharedConnection(_connection));
	}
}
