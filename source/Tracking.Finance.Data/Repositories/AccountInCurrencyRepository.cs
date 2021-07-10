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
	public sealed class AccountInCurrencyRepository : IRepository<AccountInCurrency>, IDisposable
	{
		private const string _insertSql =
			"INSERT INTO accounts_in_currency (owner_id, created_by_user_id, modified_by_user_id, account_id, currency_id) VALUES (@OwnerId, @CreatedByUserId, @ModifiedByUserId, @AccountId, @CurrencyId) RETURNING id";

		private const string _selectSql =
			"SELECT id, owner_id OwnerId, created_at CreatedAt, created_by_user_id CreatedByUserId, modified_at ModifiedAt, modified_by_user_id ModifiedByUserId, account_id AccountId, currency_id CurrencyId FROM accounts_in_currency";

		private const string _deleteSql = "DELETE FROM accounts_in_currency WHERE id = @id";

		private readonly IDbConnection _dbConnection;

		/// <summary>
		/// Initializes a new instance of the <see cref="AccountInCurrencyRepository"/> class with a database connection.
		/// </summary>
		/// <param name="dbConnection">The database connection for executing queries.</param>
		public AccountInCurrencyRepository(IDbConnection dbConnection)
		{
			_dbConnection = dbConnection;
		}

		/// <inheritdoc />
		public async Task<Guid> AddAsync(AccountInCurrency entity)
		{
			return await _dbConnection.QuerySingleAsync<Guid>(_insertSql, entity).ConfigureAwait(false);
		}

		/// <inheritdoc />
		public async Task<Guid> AddAsync(AccountInCurrency entity, IDbTransaction dbTransaction)
		{
			var command = new CommandDefinition(_insertSql, entity, dbTransaction);
			return await _dbConnection.QuerySingleAsync<Guid>(command).ConfigureAwait(false);
		}

		/// <inheritdoc />
		public async Task<AccountInCurrency?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " WHERE id = @id";
			var command = new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken);
			return await _dbConnection.QuerySingleOrDefaultAsync<AccountInCurrency>(command).ConfigureAwait(false);
		}

		/// <inheritdoc />
		public async Task<AccountInCurrency> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " WHERE id = @id";
			var command = new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken);
			return await _dbConnection.QuerySingleAsync<AccountInCurrency>(command).ConfigureAwait(false);
		}

		public async Task<List<AccountInCurrency>> GetByAccountIdAsync(
			Guid accountId,
			CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " WHERE account_id = @accountId";
			var command = new CommandDefinition(sql, new { accountId }, cancellationToken: cancellationToken);
			return (await _dbConnection.QueryAsync<AccountInCurrency>(command).ConfigureAwait(false)).ToList();
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
