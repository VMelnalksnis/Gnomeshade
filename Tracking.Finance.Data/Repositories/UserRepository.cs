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
	public sealed class UserRepository : IDisposable
	{
		public UserRepository(IDbConnection dbConnection)
		{
			DbConnection = dbConnection;
		}

		private static string TableName => "users";

		private static string ColumnNames => "id Id, counterparty_id CounterpartyId";

		private IDbConnection DbConnection { get; }

		public async Task<Guid> AddWithIdAsync(User entity)
		{
			var sql = $"INSERT INTO {TableName} (id, counterparty_id) VALUES (@{nameof(User.Id)}, @{nameof(User.CounterpartyId)}) RETURNING id";
			return await DbConnection.QuerySingleAsync<Guid>(sql, entity);
		}

		public async Task<int> DeleteAsync(Guid id)
		{
			var sql = @$"DELETE FROM {TableName} WHERE id = @id";

			return await DbConnection.ExecuteAsync(sql, new { id });
		}

		public async Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			var sql = @$"SELECT {ColumnNames} FROM {TableName}";
			var commandDefinition = new CommandDefinition(sql, cancellationToken: cancellationToken);

			var entities = await DbConnection.QueryAsync<User>(commandDefinition);
			return entities.ToList();
		}

		public async Task<User> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			var sql = @$"SELECT {ColumnNames} FROM {TableName} WHERE id = @id";
			var commandDefinition = new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken);

			return await DbConnection.QuerySingleAsync<User>(commandDefinition);
		}

		public async Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			var sql = @$"SELECT {ColumnNames} FROM {TableName} WHERE id = @id";
			var commandDefinition = new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken);

			return await DbConnection.QuerySingleOrDefaultAsync<User>(commandDefinition);
		}

		/// <inheritdoc/>
		public void Dispose() => DbConnection.Dispose();
	}
}
