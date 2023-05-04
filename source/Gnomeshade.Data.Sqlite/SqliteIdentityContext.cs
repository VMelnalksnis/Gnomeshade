// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Data.Common;

using Gnomeshade.Data.Identity;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gnomeshade.Data.Sqlite;

/// <inheritdoc />
public sealed class SqliteIdentityContext : IdentityContext
{
	private readonly DbConnection _dbConnection;

	/// <summary>Initializes a new instance of the <see cref="SqliteIdentityContext"/> class.</summary>
	/// <param name="loggerFactory">The logger factory to use for identity.</param>
	/// <param name="dbConnection">The database connection to use.</param>
	public SqliteIdentityContext(ILoggerFactory loggerFactory, DbConnection dbConnection)
		: base(loggerFactory)
	{
		_dbConnection = dbConnection;
	}

	/// <inheritdoc />
	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		base.OnConfiguring(optionsBuilder);
		optionsBuilder.UseSqlite(_dbConnection);
	}
}
