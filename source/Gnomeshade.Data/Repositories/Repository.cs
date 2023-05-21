// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Dapper;

using Gnomeshade.Data.Entities.Abstractions;
using Gnomeshade.Data.Logging;

using Microsoft.Extensions.Logging;

namespace Gnomeshade.Data.Repositories;

/// <summary>A base class for entity repositories implementing common functionality.</summary>
/// <typeparam name="TEntity">The type of entity that will be queried with this repository.</typeparam>
public abstract class Repository<TEntity>
	where TEntity : class, IEntity
{
	/// <summary>Initializes a new instance of the <see cref="Repository{TEntity}"/> class with a database connection.</summary>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	protected Repository(ILogger<Repository<TEntity>> logger, DbConnection dbConnection)
	{
		Logger = logger;
		DbConnection = dbConnection;
	}

	protected ILogger Logger { get; }

	/// <summary>Gets the database connection for executing queries.</summary>
	protected DbConnection DbConnection { get; }

	protected virtual string AccessSql => "ownerships.user_id = @ownerId AND (access.normalized_name = 'READ' OR access.normalized_name = 'OWNER')";

	protected virtual string WriteAccessSql => "ownerships.user_id = @ownerId AND (access.normalized_name = 'WRITE' OR access.normalized_name = 'OWNER')";

	protected virtual string DeleteAccessSql => "ownerships.user_id = @ownerId AND (access.normalized_name = 'DELETE' OR access.normalized_name = 'OWNER')";

	/// <summary>Gets the SQL query for deleting entities.</summary>
	protected abstract string DeleteSql { get; }

	/// <summary>Gets the SQL query for inserting a single entity.</summary>
	protected abstract string InsertSql { get; }

	/// <summary>Gets the SQL query for getting entities.</summary>
	protected abstract string SelectSql { get; }

	/// <summary>Gets the SQL query to append to <see cref="SelectSql"/> to filter for a single entity.</summary>
	protected virtual string FindSql => "WHERE Id = @id";

	/// <summary>Gets the SQL query for updating entities.</summary>
	protected abstract string UpdateSql { get; }

	/// <summary>Gets the SQL for filtering out deleted rows.</summary>
	protected virtual string NotDeleted => "deleted_at IS NULL";

	/// <summary>Adds a new entity.</summary>
	/// <param name="entity">The entity to add.</param>
	/// <returns>The id of the created entity.</returns>
	public Task<Guid> AddAsync(TEntity entity)
	{
		Logger.AddingEntity();
		return DbConnection.QuerySingleAsync<Guid>(InsertSql, entity);
	}

	/// <summary>Adds a new entity using the specified database transaction.</summary>
	/// <param name="entity">The entity to add.</param>
	/// <param name="dbTransaction">The database transaction to use for the query.</param>
	/// <returns>The id of the created entity.</returns>
	public Task<Guid> AddAsync(TEntity entity, DbTransaction dbTransaction)
	{
		Logger.AddingEntityWithTransaction();
		var command = new CommandDefinition(InsertSql, entity, dbTransaction);
		return DbConnection.QuerySingleAsync<Guid>(command);
	}

	/// <summary>Deletes the entity with the specified id.</summary>
	/// <param name="id">The id of the entity to delete.</param>
	/// <param name="ownerId">The id of the owner of the entity.</param>
	/// <returns>The number of affected rows.</returns>
	public Task DeleteAsync(Guid id, Guid ownerId)
	{
		Logger.DeletingEntity(id);
		return DbConnection.ExecuteAsync(DeleteSql, new { id, ownerId });
	}

	/// <summary>Deletes the entity with the specified id using the specified database transaction.</summary>
	/// <param name="id">The id of the entity to delete.</param>
	/// <param name="ownerId">The id of the owner of the entity.</param>
	/// <param name="dbTransaction">The database transaction to use for the query.</param>
	/// <returns>The number of affected rows.</returns>
	public Task DeleteAsync(Guid id, Guid ownerId, DbTransaction dbTransaction)
	{
		Logger.DeletingEntityWithTransaction(id);
		var command = new CommandDefinition(DeleteSql, new { id, ownerId }, dbTransaction);
		return DbConnection.ExecuteAsync(command);
	}

	/// <summary>Gets all entities.</summary>
	/// <param name="ownerId">The id of the owner of the entity.</param>
	/// <param name="includeDeleted">Whether to include deleted entities.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>A collection of all entities.</returns>
	public Task<IEnumerable<TEntity>> GetAllAsync(
		Guid ownerId,
		bool includeDeleted = false,
		CancellationToken cancellationToken = default)
	{
		Logger.GetAll(includeDeleted);
		var command = includeDeleted
			? new CommandDefinition($"{SelectSql} WHERE {AccessSql}", new { ownerId }, cancellationToken: cancellationToken)
			: new($"{SelectSql} WHERE {NotDeleted} AND {AccessSql}", new { ownerId }, cancellationToken: cancellationToken);

		return GetEntitiesAsync(command);
	}

	/// <summary>Gets all entities ignoring access control.</summary>
	/// <param name="includeDeleted">Whether to include deleted entities.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>A collection of all entities.</returns>
	public Task<IEnumerable<TEntity>> GetAllAsync(
		bool includeDeleted = false,
		CancellationToken cancellationToken = default)
	{
		Logger.GetAll(includeDeleted);
		var command = includeDeleted
			? new CommandDefinition($"{SelectSql}", cancellationToken: cancellationToken)
			: new($"{SelectSql} WHERE {NotDeleted}", cancellationToken: cancellationToken);

		return GetEntitiesAsync(command);
	}

	/// <summary>Gets an entity with the specified id.</summary>
	/// <param name="id">The id of the entity to get.</param>
	/// <param name="ownerId">The id of the owner of the entity.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>The entity with the specified id.</returns>
	public Task<TEntity> GetByIdAsync(Guid id, Guid ownerId, CancellationToken cancellationToken = default)
	{
		Logger.GetId(id);
		var sql = $"{SelectSql} {FindSql} AND {NotDeleted} AND {AccessSql}";
		var command = new CommandDefinition(sql, new { id, ownerId }, cancellationToken: cancellationToken);
		return GetAsync(command);
	}

	/// <summary>Gets an entity with the specified id using the specified database transaction.</summary>
	/// <param name="id">The id of the entity to get.</param>
	/// <param name="ownerId">The id of the owner of the entity.</param>
	/// <param name="dbTransaction">The database transaction to use for the query.</param>
	/// <returns>The entity with the specified id.</returns>
	public Task<TEntity> GetByIdAsync(Guid id, Guid ownerId, DbTransaction dbTransaction)
	{
		Logger.GetIdWithTransaction(id);
		var sql = $"{SelectSql} {FindSql} AND {NotDeleted} AND {AccessSql}";
		var command = new CommandDefinition(sql, new { id, ownerId }, dbTransaction);
		return GetAsync(command);
	}

	/// <summary>Searches for an entity with the specified id.</summary>
	/// <param name="id">The id to to search by.</param>
	/// <param name="ownerId">The id of the owner of the entity.</param>
	/// <param name="accessLevel">The access level to check.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>The entity if one exists, otherwise <see langword="null"/>.</returns>
	public Task<TEntity?> FindByIdAsync(
		Guid id,
		Guid ownerId,
		AccessLevel accessLevel = AccessLevel.Read,
		CancellationToken cancellationToken = default)
	{
		Logger.FindId(id, accessLevel);
		var sql = GetAccessSql(accessLevel);
		var command = new CommandDefinition(sql, new { id, ownerId }, cancellationToken: cancellationToken);
		return FindAsync(command);
	}

	/// <summary>Searches for an entity with the specified id using the specified database transaction.</summary>
	/// <param name="id">The id to to search by.</param>
	/// <param name="ownerId">The id of the owner of the entity.</param>
	/// <param name="dbTransaction">The database transaction to use for the query.</param>
	/// <param name="accessLevel">The access level to check.</param>
	/// <returns>The entity if one exists, otherwise <see langword="null"/>.</returns>
	public Task<TEntity?> FindByIdAsync(Guid id, Guid ownerId, DbTransaction dbTransaction, AccessLevel accessLevel = AccessLevel.Read)
	{
		Logger.FindIdWithTransaction(id, accessLevel);
		var sql = GetAccessSql(accessLevel);
		var command = new CommandDefinition(sql, new { id, ownerId }, dbTransaction);
		return FindAsync(command);
	}

	/// <summary>Searches for an entity with the specified id.</summary>
	/// <param name="id">The id to to search by.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>The entity if one exists, otherwise <see langword="null"/>.</returns>
	public Task<TEntity?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		Logger.FindId(id);
		var sql = $"{SelectSql} {FindSql} AND {NotDeleted};";
		var command = new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken);
		return FindAsync(command);
	}

	/// <summary>Searches for an entity with the specified id.</summary>
	/// <param name="id">The id to to search by.</param>
	/// <param name="dbTransaction">The database transaction to use for the query.</param>
	/// <returns>The entity if one exists, otherwise <see langword="null"/>.</returns>
	public Task<TEntity?> FindByIdAsync(Guid id, DbTransaction dbTransaction)
	{
		Logger.FindIdWithTransaction(id);
		var sql = $"{SelectSql} {FindSql} AND {NotDeleted};";
		var command = new CommandDefinition(sql, new { id }, dbTransaction);
		return FindAsync(command);
	}

	/// <summary>Updates an existing entity with the specified id.</summary>
	/// <param name="entity">The entity to update.</param>
	/// <returns>The number of affected rows.</returns>
	public Task UpdateAsync(TEntity entity)
	{
		Logger.UpdatingEntity();
		return DbConnection.ExecuteAsync(UpdateSql, entity);
	}

	/// <summary>Updates an existing entity with the specified id using the specified database transaction.</summary>
	/// <param name="entity">The entity to update.</param>
	/// <param name="dbTransaction">The database transaction to use for the query.</param>
	/// <returns>The number of affected rows.</returns>
	public Task UpdateAsync(TEntity entity, DbTransaction dbTransaction)
	{
		Logger.UpdatingEntityWithTransaction();
		return DbConnection.ExecuteAsync(UpdateSql, entity, dbTransaction);
	}

	/// <summary>Executes the specified command and maps the resulting rows to <typeparamref name="TEntity"/>.</summary>
	/// <param name="command">The command to execute.</param>
	/// <returns>The entities returned by the query.</returns>
	protected virtual Task<IEnumerable<TEntity>> GetEntitiesAsync(CommandDefinition command)
	{
		return DbConnection.QueryAsync<TEntity>(command);
	}

	/// <summary>Executes the specified command and maps the resulting row to <typeparamref name="TEntity"/>.</summary>
	/// <param name="command">The command to execute.</param>
	/// <returns>The single entity if one was returned by the query, otherwise <see langword="null"/>.</returns>
	/// <exception cref="InvalidOperationException">The query returned multiple results.</exception>
	protected async Task<TEntity?> FindAsync(CommandDefinition command)
	{
		var entities = await GetEntitiesAsync(command).ConfigureAwait(false);
		return entities
			.DistinctBy(entity => entity.Id)
			.Select(entity =>
			{
				Logger.FoundEntity(entity.Id, entity.DeletedAt);
				return entity;
			})
			.SingleOrDefault();
	}

	/// <summary>Executes the specified command and maps the resulting row to <typeparamref name="TEntity"/>.</summary>
	/// <param name="command">The command to execute.</param>
	/// <returns>The single entity returned by the query.</returns>
	/// <exception cref="InvalidOperationException">The query returned multiple or no results.</exception>
	protected async Task<TEntity> GetAsync(CommandDefinition command)
	{
		var entities = await GetEntitiesAsync(command).ConfigureAwait(false);
		return entities.DistinctBy(entity => entity.Id).Single();
	}

	private string GetAccessSql(AccessLevel accessLevel) => accessLevel switch
	{
		AccessLevel.Read => $"{SelectSql} {FindSql} AND {NotDeleted} AND {AccessSql}",
		AccessLevel.Write => $"{SelectSql} {FindSql} AND {NotDeleted} AND {WriteAccessSql}",
		AccessLevel.Delete => $"{SelectSql} {FindSql} AND {NotDeleted} AND {DeleteAccessSql}",
		_ => throw new ArgumentOutOfRangeException(nameof(accessLevel), accessLevel, null),
	};
}
