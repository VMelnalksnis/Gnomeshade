// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

using Dapper;

using Gnomeshade.Data.Entities;

namespace Gnomeshade.Data.Repositories;

/// <summary>Persistence store of <see cref="AccessEntity"/>.</summary>
public sealed class AccessRepository : IDisposable
{
	private readonly DbConnection _dbConnection;

	/// <summary>Initializes a new instance of the <see cref="AccessRepository"/> class.</summary>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public AccessRepository(DbConnection dbConnection)
	{
		_dbConnection = dbConnection;
	}

	/// <summary>Gets all accesses.</summary>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>A collection of all accesses.</returns>
	public Task<IEnumerable<AccessEntity>> GetAllAsync(CancellationToken cancellationToken)
	{
		const string sql = "SELECT id AS Id, created_at CreatedAt, name AS Name, normalized_name NormalizedName FROM access;";
		var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
		return _dbConnection.QueryAsync<AccessEntity>(command);
	}

	/// <inheritdoc />
	public void Dispose() => _dbConnection.Dispose();
}
