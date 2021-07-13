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
	public sealed class UnitRepository : IRepository<Unit>, IDisposable
	{
		private const string _insertSql =
			"INSERT INTO units (owner_id, created_by_user_id, modified_by_user_id, name, normalized_name, parent_unit_id, multiplier) VALUES (@OwnerId, @CreatedByUserId, @ModifiedByUserId, @Name, @NormalizedName, @ParentUnitId, @Multiplier) RETURNING id";

		private const string _selectSql =
			"SELECT id, created_at CreatedAt, owner_id OwnerId, created_by_user_id CreatedByUserId, modified_at ModifiedAt, modified_by_user_id ModifiedByUserId, name, normalized_name NormalizedName, parent_unit_id ParentUnitId, multiplier FROM units";

		private const string _deleteSql = "DELETE FROM units WHERE id = @Id";

		private readonly IDbConnection _dbConnection;

		/// <summary>
		/// Initializes a new instance of the <see cref="UnitRepository"/> class with a database connection.
		/// </summary>
		/// <param name="dbConnection">The database connection for executing queries.</param>
		public UnitRepository(IDbConnection dbConnection)
		{
			_dbConnection = dbConnection;
		}

		/// <inheritdoc />
		public async Task<Guid> AddAsync(Unit entity)
		{
			return await _dbConnection.QuerySingleAsync<Guid>(_insertSql, entity).ConfigureAwait(false);
		}

		/// <inheritdoc />
		public async Task<Guid> AddAsync(Unit entity, IDbTransaction dbTransaction)
		{
			var command = new CommandDefinition(_insertSql, entity, dbTransaction);
			return await _dbConnection.QuerySingleAsync<Guid>(command).ConfigureAwait(false);
		}

		/// <inheritdoc />
		public async Task<Unit?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " WHERE id = @id";
			var command = new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken);
			return await _dbConnection.QuerySingleOrDefaultAsync<Unit>(command).ConfigureAwait(false);
		}

		/// <inheritdoc />
		public async Task<Unit> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " WHERE id = @id";
			var command = new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken);
			return await _dbConnection.QuerySingleAsync<Unit>(command).ConfigureAwait(false);
		}

		public async Task<List<Unit>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			var command = new CommandDefinition(_selectSql, cancellationToken: cancellationToken);
			var units = await _dbConnection.QueryAsync<Unit>(command).ConfigureAwait(false);
			return units.ToList();
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
