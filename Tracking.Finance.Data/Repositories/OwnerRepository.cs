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
	public sealed class OwnerRepository : IDisposable
	{
		private readonly string _columnNames;
		private readonly string _tableName;
		private readonly string _insertSql;

		public OwnerRepository(IDbConnection dbConnection)
		{
			DbConnection = dbConnection;
			_columnNames = "id Id";
			_tableName = "owners";
			_insertSql = $"INSERT INTO {_tableName} VALUES (DEFAULT) RETURNING id";
		}

		private IDbConnection DbConnection { get; }

		public async Task<Guid> AddAsync(Owner entity)
		{
			return await DbConnection.QuerySingleAsync<Guid>(_insertSql, entity);
		}

		public async Task<int> DeleteAsync(int id)
		{
			var sql = @$"DELETE FROM {_tableName} WHERE id = @id";

			return await DbConnection.ExecuteAsync(sql, new { id });
		}

		public async Task<List<Owner>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			var sql = @$"SELECT {_columnNames} FROM {_tableName}";
			var commandDefinition = new CommandDefinition(sql, cancellationToken: cancellationToken);

			var entities = await DbConnection.QueryAsync<Owner>(commandDefinition);
			return entities.ToList();
		}

		public async Task<Owner> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			var sql = @$"SELECT {_columnNames} FROM {_tableName} WHERE id = @id";
			var commandDefinition = new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken);

			return await DbConnection.QuerySingleAsync<Owner>(commandDefinition);
		}

		public async Task<Owner?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			var sql = @$"SELECT {_columnNames} FROM {_tableName} WHERE id = @id";
			var commandDefinition = new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken);

			return await DbConnection.QuerySingleOrDefaultAsync<Owner>(commandDefinition);
		}

		/// <inheritdoc/>
		public void Dispose() => DbConnection.Dispose();
	}
}
