// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Data.Common;

using Gnomeshade.Data.Identity;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gnomeshade.Data.PostgreSQL;

/// <inheritdoc />
public sealed class PostgreSQLIdentityContext : IdentityContext
{
	private readonly DbDataSource _dbDataSource;

	/// <summary>Initializes a new instance of the <see cref="PostgreSQLIdentityContext"/> class.</summary>
	/// <param name="loggerFactory">The logger factory to use for identity.</param>
	/// <param name="dbDataSource">The database data source to use.</param>
	public PostgreSQLIdentityContext(ILoggerFactory loggerFactory, DbDataSource dbDataSource)
		: base(loggerFactory)
	{
		_dbDataSource = dbDataSource;
	}

	/// <inheritdoc />
	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		base.OnConfiguring(optionsBuilder);
		optionsBuilder.UseNpgsql(
			_dbDataSource,
			npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(MigrationHistoryTableName, SchemaName));
	}
}
