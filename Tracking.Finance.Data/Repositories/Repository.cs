// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Dapper;

using Tracking.Finance.Data.Models.Abstractions;

namespace Tracking.Finance.Data.Repositories
{
	/// <summary>
	/// A base class implementing <see cref="IRepository{TEntity}"/> with implementation of CRUD operations that are the same for all entities.
	/// </summary>
	public abstract class Repository<TEntity> : IRepository<TEntity>
		where TEntity : class, IEntity
	{
		/// <summary>
		/// Gets the name of the database table. It is directly inserted in SQL queries.
		/// </summary>
		protected abstract string TableName { get; }

		protected abstract string ColumnNames { get; }

		/// <summary>
		/// Gets a <see cref="IDbConnection"/> that is used to manage the database entities.
		/// </summary>
		protected IDbConnection DbConnection { get; }

		protected Repository(IDbConnection dbConnection)
		{
			DbConnection = dbConnection;
		}

		/// <inheritdoc/>
		public abstract Task<int> AddAsync(TEntity entity);

		/// <inheritdoc/>
		public virtual async Task<int> DeleteAsync(int id)
		{
			var sql = @$"DELETE FROM {TableName} WHERE id = @id";

			return await DbConnection.ExecuteAsync(sql, new { id });
		}

		/// <inheritdoc/>
		public virtual async Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			var sql = @$"SELECT {ColumnNames} FROM {TableName}";
			var commandDefinition = new CommandDefinition(sql, cancellationToken: cancellationToken);

			var entities = await DbConnection.QueryAsync<TEntity>(commandDefinition);
			return entities.ToList();
		}

		/// <inheritdoc/>
		public virtual async Task<TEntity> GetByIdAsync(int id, CancellationToken cancellationToken = default)
		{
			var sql = @$"SELECT {ColumnNames} FROM {TableName} WHERE id = @id";
			var commandDefinition = new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken);

			return await DbConnection.QuerySingleAsync<TEntity>(commandDefinition);
		}

		/// <inheritdoc/>
		public virtual async Task<TEntity?> FindByIdAsync(int id, CancellationToken cancellationToken = default)
		{
			var sql = @$"SELECT {ColumnNames} FROM {TableName} WHERE id = @id";
			var commandDefinition = new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken);

			return await DbConnection.QuerySingleOrDefaultAsync<TEntity>(commandDefinition);
		}
	}
}
