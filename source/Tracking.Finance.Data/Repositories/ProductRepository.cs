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

using Tracking.Finance.Data.Models;

namespace Tracking.Finance.Data.Repositories
{
	public sealed class ProductRepository : IRepository<Product>, IDisposable
	{
		private const string _insertSql =
			"INSERT INTO products (owner_id, created_by_user_id, modified_by_user_id, name, normalized_name, description) VALUES (@OwnerId, @CreatedByUserId, @ModifiedByUserId, @Name, @NormalizedName, @Description) RETURNING id";

		private const string _selectSql =
			"SELECT id, created_at CreatedAt, owner_id OwnerId, created_by_user_id CreatedByUserId, modified_at ModifiedAt, modified_by_user_id ModifiedByUserId, name, normalized_name NormalizedName, description FROM products";

		private const string _deleteSql = "DELETE FROM products WHERE id = @Id";

		private readonly IDbConnection _dbConnection;

		/// <summary>
		/// Initializes a new instance of the <see cref="ProductRepository"/> class with a database connection.
		/// </summary>
		/// <param name="dbConnection">The database connection for executing queries.</param>
		public ProductRepository(IDbConnection dbConnection)
		{
			_dbConnection = dbConnection;
		}

		/// <inheritdoc />
		public async Task<Guid> AddAsync(Product entity)
		{
			return await _dbConnection.QuerySingleAsync<Guid>(_insertSql, entity).ConfigureAwait(false);
		}

		/// <inheritdoc />
		public async Task<Guid> AddAsync(Product entity, IDbTransaction dbTransaction)
		{
			var command = new CommandDefinition(_insertSql, entity, dbTransaction);
			return await _dbConnection.QuerySingleAsync<Guid>(command).ConfigureAwait(false);
		}

		/// <inheritdoc />
		public async Task<Product?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " WHERE id = @id";
			var command = new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken);
			return await _dbConnection.QuerySingleOrDefaultAsync<Product>(command).ConfigureAwait(false);
		}

		/// <inheritdoc />
		public async Task<Product> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " WHERE id = @id";
			var command = new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken);
			return await _dbConnection.QuerySingleAsync<Product>(command).ConfigureAwait(false);
		}

		public async Task<List<Product>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			var command = new CommandDefinition(_selectSql, cancellationToken: cancellationToken);
			var accounts = await _dbConnection.QueryAsync<Product>(command).ConfigureAwait(false);
			return accounts.ToList();
		}

		/// <inheritdoc />
		public async Task<int> DeleteAsync(Guid id)
		{
			return await _dbConnection.ExecuteAsync(_deleteSql, new { id }).ConfigureAwait(false);
		}

		/// <inheritdoc />
		public void Dispose() => _dbConnection.Dispose();
	}
}
