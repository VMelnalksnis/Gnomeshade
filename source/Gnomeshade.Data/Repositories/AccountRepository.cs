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

using Gnomeshade.Data.Models;
using Gnomeshade.Data.Repositories.Extensions;

namespace Gnomeshade.Data.Repositories
{
	public sealed class AccountRepository : IDisposable
	{
		private const string _insertSql =
			"INSERT INTO accounts (owner_id, created_by_user_id, modified_by_user_id, name, normalized_name, preferred_currency_id, disabled_at, disabled_by_user_id, bic, iban, account_number) VALUES (@OwnerId, @CreatedByUserId, @ModifiedByUserId, @Name, @NormalizedName, @PreferredCurrencyId, @DisabledAt, @DisabledByUserId, @Bic, @Iban, @AccountNumber) RETURNING id;";

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
			"a.disabled_at DisabledAt," +
			"a.disabled_by_user_id DisabledByUserId," +
			"a.bic, " +
			"a.iban, " +
			"a.account_number AccountNumber, " +
			"pc.id, " +
			"pc.created_at CreatedAt, " +
			"pc.name, " +
			"pc.normalized_name NormalizedName, " +
			"pc.numeric_code NumericCode, " +
			"pc.alphabetic_code AlphabeticCode, " +
			"pc.minor_unit MinorUnit, " +
			"pc.official, " +
			"pc.crypto, " +
			"pc.historical, " +
			"pc.active_from ActiveFrom, " +
			"pc.active_until ActiveUntil, " +
			"aic.id, " +
			"aic.created_at CreatedAt, " +
			"aic.owner_id OwnerId, " +
			"aic.created_by_user_id CreatedByUserId, " +
			"aic.modified_at ModifiedAt, " +
			"aic.modified_by_user_id ModifiedByUserId, " +
			"aic.account_id AccountId, " +
			"aic.currency_id CurrencyId, " +
			"aic.disabled_at DisabledAt," +
			"aic.disabled_by_user_id DisabledByUserId," +
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
			"LEFT JOIN currencies pc ON a.preferred_currency_id = pc.id " +
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

		public Task<Guid> AddAsync(Account entity, IDbTransaction dbTransaction)
		{
			var command = new CommandDefinition(_insertSql, entity, dbTransaction);
			return _dbConnection.QuerySingleAsync<Guid>(command);
		}

		public Task<Account?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " WHERE a.id = @id;";
			var command = new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken);
			return FindAsync(command);
		}

		public Task<Account?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " WHERE a.normalized_name = @name;";
			var command = new CommandDefinition(sql, new { name }, cancellationToken: cancellationToken);
			return FindAsync(command);
		}

		public Task<Account?> FindByIbanAsync(string iban, CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " WHERE a.iban = @iban;";
			var command = new CommandDefinition(sql, new { iban }, cancellationToken: cancellationToken);
			return FindAsync(command);
		}

		public async Task<Account> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " WHERE a.id = @id;";
			var command = new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken);

			var accounts = await GetAccountsAsync(command).ConfigureAwait(false);
			return accounts.Single();
		}

		public Task<IEnumerable<Account>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " ORDER BY a.created_at DESC LIMIT 1000;";
			var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
			return GetAccountsAsync(command);
		}

		/// <summary>
		/// Gets all accounts accounts that have not been disabled, with currencies which also have not been disabled.
		/// </summary>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
		/// <returns>A collection of all active accounts.</returns>
		public Task<IEnumerable<Account>> GetAllActiveAsync(CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " WHERE a.disabled_at IS NULL AND aic.disabled_at IS NULL ORDER BY a.created_at;";
			var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
			return GetAccountsAsync(command);
		}

		public Task<int> DeleteAsync(Guid id, IDbTransaction dbTransaction)
		{
			var command = new CommandDefinition(_deleteSql, new { id }, dbTransaction);
			return _dbConnection.ExecuteAsync(command);
		}

		/// <inheritdoc />
		public void Dispose() => _dbConnection.Dispose();

		private async Task<Account?> FindAsync(CommandDefinition command)
		{
			var accounts = await GetAccountsAsync(command).ConfigureAwait(false);
			return accounts.SingleOrDefault();
		}

		private async Task<IEnumerable<Account>> GetAccountsAsync(
			CommandDefinition command)
		{
			var oneToOnes =
				await _dbConnection
					.QueryAsync<Account, Currency, AccountInCurrency, Currency, OneToOne<Account, AccountInCurrency>>(
						command,
						(account, preferredCurrency, inCurrency, currency) =>
						{
							account.PreferredCurrency = preferredCurrency;
							inCurrency.Currency = currency;
							return new(account, inCurrency);
						})
					.ConfigureAwait(false);

			return oneToOnes.GroupBy(oneToOne => oneToOne.First.Id).Select(grouping => Account.FromGrouping(grouping));
		}
	}
}
