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

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

using static Gnomeshade.Data.Repositories.AccessLevel;

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

	/// <summary>Gets the SQL query for deleting entities.</summary>
	protected abstract string DeleteSql { get; }

	/// <summary>Gets the SQL query for inserting a single entity.</summary>
	protected abstract string InsertSql { get; }

	protected abstract string SelectSql { get; }

	protected string SelectActiveSql => $"{SelectSql} AND {NotDeleted}";

	/// <summary>Gets the SQL query for getting entities.</summary>
	protected abstract string SelectAllSql { get; }

	/// <summary>Gets SQL where clause that filters for specific entity by id.</summary>
	protected abstract string FindSql { get; }

	protected abstract string GroupBy { get; }

	/// <summary>Gets the SQL query for updating entities.</summary>
	protected abstract string UpdateSql { get; }

	/// <summary>Gets the SQL for filtering out deleted rows.</summary>
	protected abstract string NotDeleted { get; }

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
	/// <param name="userId">The id of the user requesting access to the entity.</param>
	/// <returns>The number of affected rows.</returns>
	[MustUseReturnValue]
	public async Task<int> DeleteAsync(Guid id, Guid userId)
	{
		Logger.DeletingEntity(id);
		var count = await DbConnection.ExecuteAsync(DeleteSql, new { id, userId });
		Logger.DeletedRows(count);
		return count;
	}

	/// <summary>Deletes the entity with the specified id using the specified database transaction.</summary>
	/// <param name="id">The id of the entity to delete.</param>
	/// <param name="userId">The id of the user requesting access to the entity.</param>
	/// <param name="dbTransaction">The database transaction to use for the query.</param>
	/// <returns>The number of affected rows.</returns>
	[MustUseReturnValue]
	public async Task<int> DeleteAsync(Guid id, Guid userId, DbTransaction dbTransaction)
	{
		Logger.DeletingEntityWithTransaction(id);
		var count = await DbConnection.ExecuteAsync(DeleteSql, new { id, userId }, dbTransaction);
		Logger.DeletedRows(count);
		return count;
	}

	/// <summary>Gets all entities.</summary>
	/// <param name="userId">The id of the user requesting access to the entity.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>A collection of all entities.</returns>
	public Task<IEnumerable<TEntity>> GetIncludingDeletedAsync(
		Guid userId,
		CancellationToken cancellationToken = default)
	{
		Logger.GetAll(true);
		return GetEntitiesAsync(new(
			$"{SelectSql} {GroupBy};",
			new { userId, access = Read.ToParam() },
			cancellationToken: cancellationToken));
	}

	public Task<IEnumerable<TEntity>> GetAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		Logger.GetAll();
		return GetEntitiesAsync(new(
			$"{SelectActiveSql} {GroupBy};",
			new { userId, access = Read.ToParam() },
			cancellationToken: cancellationToken));
	}

	/// <summary>Gets an entity with the specified id.</summary>
	/// <param name="id">The id of the entity to get.</param>
	/// <param name="userId">The id of the user requesting access to the entity.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>The entity with the specified id.</returns>
	public Task<TEntity> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
	{
		Logger.GetId(id);
		return GetAsync(new(
			$"{SelectActiveSql} AND {FindSql} {GroupBy};",
			new { id, userId, access = Read.ToParam() },
			cancellationToken: cancellationToken));
	}

	/// <summary>Gets an entity with the specified id using the specified database transaction.</summary>
	/// <param name="id">The id of the entity to get.</param>
	/// <param name="userId">The id of the user requesting access to the entity.</param>
	/// <param name="dbTransaction">The database transaction to use for the query.</param>
	/// <returns>The entity with the specified id.</returns>
	public Task<TEntity> GetByIdAsync(Guid id, Guid userId, DbTransaction dbTransaction)
	{
		Logger.GetIdWithTransaction(id);
		return GetAsync(new(
			$"{SelectActiveSql} AND {FindSql} {GroupBy};",
			new { id, userId, access = Read.ToParam() },
			dbTransaction));
	}

	/// <summary>Searches for an entity with the specified id.</summary>
	/// <param name="id">The id to to search by.</param>
	/// <param name="userId">The id of the user requesting access to the entity.</param>
	/// <param name="accessLevel">The access level to check.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>The entity if one exists, otherwise <see langword="null"/>.</returns>
	public Task<TEntity?> FindByIdAsync(
		Guid id,
		Guid userId,
		AccessLevel accessLevel = Read,
		CancellationToken cancellationToken = default)
	{
		Logger.FindId(id, accessLevel);
		return FindAsync(new(
			$"{SelectActiveSql} AND {FindSql} {GroupBy};",
			new { id, userId, access = accessLevel.ToParam() },
			cancellationToken: cancellationToken));
	}

	/// <summary>Searches for an entity with the specified id using the specified database transaction.</summary>
	/// <param name="id">The id to to search by.</param>
	/// <param name="userId">The id of the user requesting access to the entity.</param>
	/// <param name="dbTransaction">The database transaction to use for the query.</param>
	/// <param name="accessLevel">The access level to check.</param>
	/// <returns>The entity if one exists, otherwise <see langword="null"/>.</returns>
	public Task<TEntity?> FindByIdAsync(
		Guid id,
		Guid userId,
		DbTransaction dbTransaction,
		AccessLevel accessLevel = Read)
	{
		Logger.FindIdWithTransaction(id, accessLevel);
		return FindAsync(new(
			$"{SelectActiveSql} AND {FindSql} {GroupBy};",
			new { id, userId, access = accessLevel.ToParam() },
			dbTransaction));
	}

	/// <summary>Searches for an entity with the specified id.</summary>
	/// <param name="id">The id to to search by.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>The entity if one exists, otherwise <see langword="null"/>.</returns>
	public Task<TEntity?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		Logger.FindId(id);
		return FindAsync(new(
			$"{SelectAllSql} WHERE {FindSql} AND {NotDeleted} {GroupBy};",
			new { id },
			cancellationToken: cancellationToken));
	}

	/// <summary>Searches for an entity with the specified id.</summary>
	/// <param name="id">The id to to search by.</param>
	/// <param name="dbTransaction">The database transaction to use for the query.</param>
	/// <returns>The entity if one exists, otherwise <see langword="null"/>.</returns>
	public Task<TEntity?> FindByIdAsync(Guid id, DbTransaction dbTransaction)
	{
		Logger.FindIdWithTransaction(id);
		return FindAsync(new(
			$"{SelectAllSql} WHERE {FindSql} AND {NotDeleted} {GroupBy};",
			new { id },
			dbTransaction));
	}

	/// <summary>Updates an existing entity with the specified id.</summary>
	/// <param name="entity">The entity to update.</param>
	/// <returns>The number of affected rows.</returns>
	[MustUseReturnValue]
	public async Task<int> UpdateAsync(TEntity entity)
	{
		Logger.UpdatingEntity();
		var count = await DbConnection.ExecuteAsync(UpdateSql, entity);
		Logger.UpdatedRows(count);
		return count;
	}

	/// <summary>Updates an existing entity with the specified id using the specified database transaction.</summary>
	/// <param name="entity">The entity to update.</param>
	/// <param name="dbTransaction">The database transaction to use for the query.</param>
	/// <returns>The number of affected rows.</returns>
	[MustUseReturnValue]
	public async Task<int> UpdateAsync(TEntity entity, DbTransaction dbTransaction)
	{
		Logger.UpdatingEntityWithTransaction();
		var count = await DbConnection.ExecuteAsync(UpdateSql, entity, dbTransaction);
		Logger.UpdatedRows(count);
		return count;
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
		return entities.Single();
	}
}
