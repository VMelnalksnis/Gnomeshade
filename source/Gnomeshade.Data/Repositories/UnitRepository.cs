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

namespace Gnomeshade.Data.Repositories
{
	/// <summary>
	/// Database backed <see cref="UnitEntity"/> repository.
	/// </summary>
	public sealed class UnitRepository : IDisposable
	{
		private const string _insertSql =
			"INSERT INTO units (owner_id, created_by_user_id, modified_by_user_id, name, normalized_name, parent_unit_id, multiplier) VALUES (@OwnerId, @CreatedByUserId, @ModifiedByUserId, @Name, @NormalizedName, @ParentUnitId, @Multiplier) RETURNING id";

		private const string _selectSql =
			"SELECT id, created_at CreatedAt, owner_id OwnerId, created_by_user_id CreatedByUserId, modified_at ModifiedAt, modified_by_user_id ModifiedByUserId, name, normalized_name NormalizedName, parent_unit_id ParentUnitId, multiplier FROM units";

		private const string _deleteSql = "DELETE FROM units WHERE id = @id";

		private readonly IDbConnection _dbConnection;

		/// <summary>
		/// Initializes a new instance of the <see cref="UnitRepository"/> class with a database connection.
		/// </summary>
		/// <param name="dbConnection">The database connection for executing queries.</param>
		public UnitRepository(IDbConnection dbConnection)
		{
			_dbConnection = dbConnection;
		}

		public Task<Guid> AddAsync(UnitEntity entity)
		{
			return _dbConnection.QuerySingleAsync<Guid>(_insertSql, entity);
		}

		public Task<Guid> AddAsync(UnitEntity entity, IDbTransaction dbTransaction)
		{
			var command = new CommandDefinition(_insertSql, entity, dbTransaction);
			return _dbConnection.QuerySingleAsync<Guid>(command);
		}

		public Task<UnitEntity?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " WHERE id = @id";
			var command = new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken);
			return _dbConnection.QuerySingleOrDefaultAsync<UnitEntity>(command)!;
		}

		public Task<UnitEntity> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " WHERE id = @id";
			var command = new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken);
			return _dbConnection.QuerySingleAsync<UnitEntity>(command);
		}

		public async Task<List<UnitEntity>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			var command = new CommandDefinition(_selectSql, cancellationToken: cancellationToken);
			var units = await _dbConnection.QueryAsync<UnitEntity>(command).ConfigureAwait(false);
			return units.ToList();
		}

		public Task<int> DeleteAsync(Guid id)
		{
			return _dbConnection.ExecuteAsync(_deleteSql, new { id });
		}

		/// <inheritdoc />
		public void Dispose() => _dbConnection.Dispose();
	}
}
