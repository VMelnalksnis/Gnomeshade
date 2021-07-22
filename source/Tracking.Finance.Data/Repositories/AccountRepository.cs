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
using Tracking.Finance.Data.Repositories.Extensions;

namespace Tracking.Finance.Data.Repositories
{
	public sealed class AccountRepository : IRepository<Account>, IDisposable
	{
		private const string _insertSql =
			"INSERT INTO accounts (owner_id, created_by_user_id, modified_by_user_id, name, normalized_name, preferred_currency_id, bic, iban, account_number) VALUES (@OwnerId, @CreatedByUserId, @ModifiedByUserId, @Name, @NormalizedName, @PreferredCurrencyId, @Bic, @Iban, @AccountNumber) RETURNING id;";

		private const string _selectSql =
			"SELECT a.id, " +
			"a.created_at CreatedAt, " +
			"a.owner_id OwnerId, " +
			"a.created_by_user_id CreatedByUserId, " +
			"a.modified_at ModifiedAt, " +
			"a.modified_by_user_id ModifiedByUserId, " +
			"a.name, " +
			"a.normalized_name NormalizedName, " +
			"a.preferred_currency_id PreferredCurrencyId, " +
			"a.bic, " +
			"a.iban, " +
			"a.account_number AccountNumber, " +
			"aic.id, " +
			"aic.created_at CreatedAt, " +
			"aic.owner_id OwnerId, " +
			"aic.created_by_user_id CreatedByUserId, " +
			"aic.modified_at ModifiedAt, " +
			"aic.modified_by_user_id ModifiedByUserId, " +
			"aic.account_id AccountId, " +
			"aic.currency_id CurrencyId, " +
			"c.id, " +
			"c.created_at CreatedAt, " +
			"c.name, " +
			"c.normalized_name NormalizedName, " +
			"c.numeric_code NumericCode, " +
			"c.alphabetic_code AlphabeticCode, " +
			"c.minor_unit MinorUnit, " +
			"c.official, " +
			"c.crypto, " +
			"c.historical, " +
			"c.active_from ActiveFrom, " +
			"c.active_until ActiveUntil " +
			"FROM accounts a " +
			"LEFT JOIN accounts_in_currency aic ON a.id = aic.account_id " +
			"LEFT JOIN currencies c ON aic.currency_id = c.id";

		private const string _deleteSql = "DELETE FROM accounts WHERE id = @Id;";

		private readonly IDbConnection _dbConnection;

		/// <summary>
		/// Initializes a new instance of the <see cref="AccountRepository"/> class with a database connection.
		/// </summary>
		/// <param name="dbConnection">The database connection for executing queries.</param>
		public AccountRepository(IDbConnection dbConnection)
		{
			_dbConnection = dbConnection;
		}

		/// <inheritdoc />
		public Task<Guid> AddAsync(Account entity)
		{
			return _dbConnection.QuerySingleAsync<Guid>(_insertSql, entity);
		}

		/// <inheritdoc />
		public Task<Guid> AddAsync(Account entity, IDbTransaction dbTransaction)
		{
			var command = new CommandDefinition(_insertSql, entity, dbTransaction);
			return _dbConnection.QuerySingleAsync<Guid>(command);
		}

		/// <inheritdoc />
		public async Task<Account?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " WHERE a.id = @id;";
			var command = new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken);

			var accountGroupings = await GetAccountsAsync(command).ConfigureAwait(false);
			var grouping = accountGroupings.SingleOrDefault();

			return grouping is null ? null : Account.FromGrouping(grouping);
		}

		public async Task<Account?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " WHERE a.normalized_name = @name;";
			var command = new CommandDefinition(sql, new { name }, cancellationToken: cancellationToken);

			var accountGroupings = await GetAccountsAsync(command).ConfigureAwait(false);
			var grouping = accountGroupings.SingleOrDefault();

			return grouping is null ? null : Account.FromGrouping(grouping);
		}

		/// <inheritdoc />
		public async Task<Account> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " WHERE a.id = @id;";
			var command = new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken);

			var accountGroupings = await GetAccountsAsync(command).ConfigureAwait(false);
			var grouping = accountGroupings.Single();
			return Account.FromGrouping(grouping);
		}

		public async Task<List<Account>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " ORDER BY a.created_at DESC LIMIT 1000;";
			var command = new CommandDefinition(sql, cancellationToken: cancellationToken);

			var accountGroupings = await GetAccountsAsync(command).ConfigureAwait(false);
			return accountGroupings.Select(grouping => Account.FromGrouping(grouping)).ToList();
		}

		/// <inheritdoc />
		public Task<int> DeleteAsync(Guid id)
		{
			return _dbConnection.ExecuteAsync(_deleteSql, new { id });
		}

		/// <inheritdoc />
		public void Dispose() => _dbConnection.Dispose();

		private Task<IEnumerable<IGrouping<Account, OneToOne<Account, OneToOne<AccountInCurrency, Currency>>>>>
			GetAccountsAsync(CommandDefinition command)
		{
			return _dbConnection.QueryOneToManyAsync<Account, AccountInCurrency, Currency>(command);
		}
	}
}
