// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Dapper;

using Gnomeshade.Data.Entities;

namespace Gnomeshade.Data.Repositories;

/// <summary>Persistence store of <see cref="OwnershipEntity"/>.</summary>
public sealed class OwnershipRepository : IDisposable
{
	private readonly IDbConnection _dbConnection;

	/// <summary>Initializes a new instance of the <see cref="OwnershipRepository"/> class.</summary>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public OwnershipRepository(IDbConnection dbConnection)
	{
		_dbConnection = dbConnection;
	}

	/// <summary>Searches for an entity with the specified id.</summary>
	/// <param name="id">The id to to search by.</param>
	/// <param name="userId">The id of the owner of the entity.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>The entity if one exists, otherwise <see langword="null"/>.</returns>
	public async Task<OwnershipEntity?> FindByIdAsync(
		Guid id,
		Guid userId,
		CancellationToken cancellationToken = default)
	{
		var sql = $"{Queries.Ownership.Select} WHERE id = @id AND user_id = @userId LIMIT 2;";
		var command = new CommandDefinition(sql, new { id, userId }, cancellationToken: cancellationToken);
		var entities = await _dbConnection.QueryAsync<OwnershipEntity>(command);
		return entities.SingleOrDefault();
	}

	/// <summary>Searches for an entity with the specified id.</summary>
	/// <param name="id">The id to to search by.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>The entity if one exists, otherwise <see langword="null"/>.</returns>
	public async Task<OwnershipEntity?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var sql = $"{Queries.Ownership.Select} WHERE id = @id LIMIT 2;";
		var command = new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken);
		var entities = await _dbConnection.QueryAsync<OwnershipEntity>(command);
		return entities.SingleOrDefault();
	}

	/// <summary>
	/// Adds the default <see cref="OwnershipEntity"/>, where the <see cref="OwnershipEntity.Id"/>,
	/// <see cref="OwnershipEntity.OwnerId"/> and <see cref="OwnershipEntity.UserId"/> is the id of the user.
	/// </summary>
	/// <param name="id">Id of the user.</param>
	/// <param name="dbTransaction">The database transaction to use for the query.</param>
	/// <returns>The id of the created ownership.</returns>
	public async Task AddDefaultAsync(Guid id, IDbTransaction dbTransaction)
	{
		const string text = "SELECT id AS Id FROM access WHERE normalized_name = 'OWNER'";
		var accessCommand = new CommandDefinition(text, null, dbTransaction);
		var accessId = await _dbConnection.QuerySingleAsync<Guid>(accessCommand);

		await AddAsync(new() { Id = id, OwnerId = id, UserId = id, AccessId = accessId });
	}

	/// <summary>Adds a new ownership.</summary>
	/// <param name="ownership">The ownership to add.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public Task AddAsync(OwnershipEntity ownership) =>
		_dbConnection.ExecuteAsync(Queries.Ownership.Insert, ownership);

	/// <summary>Updates an existing ownership with the specified id.</summary>
	/// <param name="ownership">The ownership to update.</param>
	/// <returns>The number of affected rows.</returns>
	public Task<int> UpdateAsync(OwnershipEntity ownership) =>
		_dbConnection.ExecuteAsync(Queries.Ownership.Update, ownership);

	/// <summary>Gets all ownerships.</summary>
	/// <param name="userId">The id of the user for which to get all ownerships.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>A collection of all ownerships.</returns>
	public Task<IEnumerable<OwnershipEntity>> GetAllAsync(Guid userId, CancellationToken cancellationToken)
	{
		var text = $"{Queries.Ownership.Select} WHERE user_id = @userId;";
		var command = new CommandDefinition(text, new { userId }, cancellationToken: cancellationToken);
		return _dbConnection.QueryAsync<OwnershipEntity>(command);
	}

	/// <summary>Deletes the entity with the specified id using the specified database transaction.</summary>
	/// <param name="id">The id of the entity to delete.</param>
	/// <returns>The number of affected rows.</returns>
	public Task<int> DeleteAsync(Guid id) =>
		_dbConnection.ExecuteAsync(new(Queries.Ownership.Delete, new { id }));

	/// <inheritdoc />
	public void Dispose() => _dbConnection.Dispose();
}
