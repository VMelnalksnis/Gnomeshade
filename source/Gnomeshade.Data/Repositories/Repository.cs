// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

using Dapper;

using Gnomeshade.Data.Models.Abstractions;

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
		/// Adds a new entity.
		/// </summary>
		/// <param name="entity">The entity to add.</param>
		/// <returns>The id of the created entity.</returns>
		public virtual Task<Guid> AddAsync(TEntity entity)
		{
			return DbConnection.QuerySingleAsync<Guid>(InsertSql, entity);
		}

		/// <summary>
		/// Adds a new entity using the specified database transaction.
		/// </summary>
		/// <param name="entity">The entity to add.</param>
		/// <param name="dbTransaction">The database transaction to use for the query.</param>
		/// <returns>The id of the created entity.</returns>
		public virtual Task<Guid> AddAsync(TEntity entity, IDbTransaction dbTransaction)
		{
			var command = new CommandDefinition(InsertSql, entity, dbTransaction);
			return DbConnection.QuerySingleAsync<Guid>(command);
		}

		/// <summary>
		/// Deletes the entity with the specified id.
		/// </summary>
		/// <param name="id">The id of the entity to delete.</param>
		/// <returns>The number of affected rows.</returns>
		public virtual Task<int> DeleteAsync(Guid id)
		{
			return DbConnection.ExecuteAsync(DeleteSql, new { id });
		}

		/// <summary>
		/// Deletes the entity with the specified id using the specified database transaction.
		/// </summary>
		/// <param name="id">The id of the entity to delete.</param>
		/// <param name="dbTransaction">The database transaction to use for the query.</param>
		/// <returns>The number of affected rows.</returns>
		public virtual Task<int> DeleteAsync(Guid id, IDbTransaction dbTransaction)
		{
			var command = new CommandDefinition(DeleteSql, new { id }, dbTransaction);
			return DbConnection.ExecuteAsync(command);
		}

		public virtual Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			var command = new CommandDefinition(SelectSql, cancellationToken: cancellationToken);
			return DbConnection.QueryAsync<TEntity>(command);
		}

		public virtual Task<TEntity> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			var sql = $"{SelectSql} WHERE id = @id;";
			var command = new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken);
			return DbConnection.QuerySingleAsync<TEntity>(command);
		}

		public virtual Task<TEntity?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			var sql = $"{SelectSql} WHERE id = @id;";
			var command = new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken);
			return DbConnection.QuerySingleOrDefaultAsync<TEntity?>(command);
		}

		/// <inheritdoc />
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				DbConnection.Dispose();
			}
		}
	}
}
