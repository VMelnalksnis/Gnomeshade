﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

using Dapper;

using Gnomeshade.Data.Entities.Abstractions;

namespace Gnomeshade.Data.Repositories
{
	/// <summary>
	/// A base class for entity repositories implementing common functionality.
	/// </summary>
	/// <typeparam name="TEntity">The type of entity that will be queried with this repository.</typeparam>
	public abstract class Repository<TEntity> : IDisposable
		where TEntity : class, IEntity
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Repository{TEntity}"/> class with a database connection.
		/// </summary>
		/// <param name="dbConnection">The database connection for executing queries.</param>
		protected Repository(IDbConnection dbConnection)
		{
			DbConnection = dbConnection;
		}

		/// <summary>
		/// Gets the database connection for executing queries.
		/// </summary>
		protected IDbConnection DbConnection { get; }

		/// <summary>
		/// Gets the SQL query for deleting entities.
		/// </summary>
		protected abstract string DeleteSql { get; }

		/// <summary>
		/// Gets the SQL query for inserting a single entity.
		/// </summary>
		protected abstract string InsertSql { get; }

		/// <summary>
		/// Gets the SQL query for getting entities.
		/// </summary>
		protected abstract string SelectSql { get; }

		/// <summary>
		/// Gets the SQL query to append to <see cref="SelectSql"/> to filter for a single entity.
		/// </summary>
		protected virtual string FindSql => "WHERE id = @id;";

		/// <summary>
		/// Gets the SQL query for updating entities.
		/// </summary>
		protected abstract string UpdateSql { get; }

		/// <summary>
		/// Adds a new entity.
		/// </summary>
		/// <param name="entity">The entity to add.</param>
		/// <returns>The id of the created entity.</returns>
		public Task<Guid> AddAsync(TEntity entity) => DbConnection.QuerySingleAsync<Guid>(InsertSql, entity);

		/// <summary>
		/// Adds a new entity using the specified database transaction.
		/// </summary>
		/// <param name="entity">The entity to add.</param>
		/// <param name="dbTransaction">The database transaction to use for the query.</param>
		/// <returns>The id of the created entity.</returns>
		public Task<Guid> AddAsync(TEntity entity, IDbTransaction dbTransaction)
		{
			var command = new CommandDefinition(InsertSql, entity, dbTransaction);
			return DbConnection.QuerySingleAsync<Guid>(command);
		}

		/// <summary>
		/// Deletes the entity with the specified id.
		/// </summary>
		/// <param name="id">The id of the entity to delete.</param>
		/// <returns>The number of affected rows.</returns>
		public Task<int> DeleteAsync(Guid id) => DbConnection.ExecuteAsync(DeleteSql, new { id });

		/// <summary>
		/// Deletes the entity with the specified id using the specified database transaction.
		/// </summary>
		/// <param name="id">The id of the entity to delete.</param>
		/// <param name="dbTransaction">The database transaction to use for the query.</param>
		/// <returns>The number of affected rows.</returns>
		public Task<int> DeleteAsync(Guid id, IDbTransaction dbTransaction)
		{
			var command = new CommandDefinition(DeleteSql, new { id }, dbTransaction);
			return DbConnection.ExecuteAsync(command);
		}

		/// <summary>
		/// Gets all entities.
		/// </summary>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
		/// <returns>A collection of all entities.</returns>
		public virtual Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			var command = new CommandDefinition(SelectSql, cancellationToken: cancellationToken);
			return DbConnection.QueryAsync<TEntity>(command);
		}

		/// <summary>
		/// Gets an entity with the specified id.
		/// </summary>
		/// <param name="id">The id of the entity to get.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
		/// <returns>The entity with the specified id.</returns>
		public virtual Task<TEntity> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			var sql = $"{SelectSql} {FindSql}";
			var command = new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken);
			return DbConnection.QuerySingleAsync<TEntity>(command);
		}

		/// <summary>
		/// Searches for an entity with the specified id.
		/// </summary>
		/// <param name="id">The id to to search by.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
		/// <returns>The entity if one exists, otherwise <see langword="null"/>.</returns>
		public virtual Task<TEntity?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			var sql = $"{SelectSql} {FindSql}";
			var command = new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken);
			return DbConnection.QuerySingleOrDefaultAsync<TEntity?>(command);
		}

		/// <summary>
		/// Updates an existing entity with the specified id.
		/// </summary>
		/// <param name="entity">The entity to update.</param>
		/// <returns>The number of affected rows.</returns>
		public Task<int> UpdateAsync(TEntity entity) => DbConnection.ExecuteAsync(UpdateSql, entity);

		/// <summary>
		/// Updates an existing entity with the specified id using the specified database transaction.
		/// </summary>
		/// <param name="entity">The entity to update.</param>
		/// <param name="dbTransaction">The database transaction to use for the query.</param>
		/// <returns>The number of affected rows.</returns>
		public Task<int> UpdateAsync(TEntity entity, IDbTransaction dbTransaction)
		{
			return DbConnection.ExecuteAsync(UpdateSql, entity, dbTransaction);
		}

		/// <inheritdoc />
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				DbConnection.Dispose();
			}
		}
	}
}
