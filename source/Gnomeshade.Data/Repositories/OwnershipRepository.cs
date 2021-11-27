// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data;
using System.Threading.Tasks;

using Dapper;

using Gnomeshade.Data.Entities;

namespace Gnomeshade.Data.Repositories;

/// <summary>
/// Database backed <see cref="OwnershipEntity"/> repository.
/// </summary>
public sealed class OwnershipRepository : IDisposable
{
	private readonly IDbConnection _dbConnection;

	/// <summary>
	/// Initializes a new instance of the <see cref="OwnershipRepository"/> class with a database connection.
	/// </summary>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public OwnershipRepository(IDbConnection dbConnection)
	{
		_dbConnection = dbConnection;
	}

	/// <summary>
	/// Adds the default <see cref="OwnershipEntity"/>, where the <see cref="OwnershipEntity.Id"/>,
	/// <see cref="OwnershipEntity.OwnerId"/> and <see cref="OwnershipEntity.UserId"/> is the id of the user.
	/// </summary>
	/// <param name="id">Id of the user.</param>
	/// <param name="dbTransaction">The database transaction to use for the query.</param>
	/// <returns>The id of the created ownership.</returns>
	public Task<Guid> AddDefaultAsync(Guid id, IDbTransaction dbTransaction)
	{
		var command = new CommandDefinition(Queries.Ownership.Insert, new { id }, dbTransaction);
		return _dbConnection.QuerySingleAsync<Guid>(command);
	}

	/// <inheritdoc />
	public void Dispose() => _dbConnection.Dispose();
}
