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
using Gnomeshade.Data.Repositories.Extensions;

namespace Gnomeshade.Data.Repositories
{
	public sealed class AccountRepository : NamedRepository<AccountEntity>
	{
		private const string _insertSql =
			"INSERT INTO accounts (owner_id, created_by_user_id, modified_by_user_id, name, normalized_name,counterparty_id, preferred_currency_id, disabled_at, disabled_by_user_id, bic, iban, account_number) VALUES (@OwnerId, @CreatedByUserId, @ModifiedByUserId, @Name, @NormalizedName, @CounterpartyId, @PreferredCurrencyId, @DisabledAt, @DisabledByUserId, @Bic, @Iban, @AccountNumber) RETURNING id;";

		private const string _selectSql =
			"SELECT a.id, " +
			"a.created_at CreatedAt, " +
			"a.owner_id OwnerId, " +
			"a.created_by_user_id CreatedByUserId, " +
			"a.modified_at ModifiedAt, " +
			"a.modified_by_user_id ModifiedByUserId, " +
			"a.name, " +
			"a.normalized_name NormalizedName, " +
			"a.counterparty_id CounterpartyId, " +
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

		/// <summary>
		/// Initializes a new instance of the <see cref="AccountRepository"/> class with a database connection.
		/// </summary>
		/// <param name="dbConnection">The database connection for executing queries.</param>
		public AccountRepository(IDbConnection dbConnection)
			: base(dbConnection)
		{
		}

		/// <inheritdoc />
		protected override string DeleteSql => _deleteSql;

		/// <inheritdoc />
		protected override string InsertSql => _insertSql;

		/// <inheritdoc />
		protected override string SelectSql => _selectSql;

		/// <inheritdoc />
		protected override string UpdateSql => throw new NotImplementedException();

		/// <inheritdoc />
		public override Task<AccountEntity?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " WHERE a.id = @id;";
			var command = new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken);
			return FindAsync(command);
		}

		/// <inheritdoc />
		public override Task<AccountEntity?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " WHERE a.normalized_name = @name;";
			var command = new CommandDefinition(sql, new { name }, cancellationToken: cancellationToken);
			return FindAsync(command);
		}

		/// <summary>
		/// Finds an account with the specified IBAN.
		/// </summary>
		/// <param name="iban">The IBAN for which to search for.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
		/// <returns>The account with the IBAN if one exists, otherwise <see langword="null"/>.</returns>
		public Task<AccountEntity?> FindByIbanAsync(string iban, CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " WHERE a.iban = @iban;";
			var command = new CommandDefinition(sql, new { iban }, cancellationToken: cancellationToken);
			return FindAsync(command);
		}

		public Task<AccountEntity?> FindByBicAsync(string bic, CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " WHERE a.bic = @bic;";
			var command = new CommandDefinition(sql, new { bic }, cancellationToken: cancellationToken);
			return FindAsync(command);
		}

		/// <inheritdoc />
		public override async Task<AccountEntity> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " WHERE a.id = @id;";
			var command = new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken);

			var accounts = await GetAccountsAsync(command).ConfigureAwait(false);
			return accounts.Single();
		}

		public async Task<AccountEntity> GetByIdAsync(Guid id, IDbTransaction dbTransaction)
		{
			const string sql = _selectSql + " WHERE a.id = @id;";
			var command = new CommandDefinition(sql, new { id }, dbTransaction);

			var accounts = await GetAccountsAsync(command).ConfigureAwait(false);
			return accounts.Single();
		}

		/// <inheritdoc />
		public override Task<IEnumerable<AccountEntity>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " ORDER BY a.created_at DESC, aic.created_at DESC LIMIT 1000;";
			var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
			return GetAccountsAsync(command);
		}

		/// <summary>
		/// Gets all accounts accounts that have not been disabled, with currencies which also have not been disabled.
		/// </summary>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
		/// <returns>A collection of all active accounts.</returns>
		public Task<IEnumerable<AccountEntity>> GetAllActiveAsync(CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " WHERE a.disabled_at IS NULL AND aic.disabled_at IS NULL ORDER BY a.created_at;";
			var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
			return GetAccountsAsync(command);
		}

		private async Task<AccountEntity?> FindAsync(CommandDefinition command)
		{
			var accounts = await GetAccountsAsync(command).ConfigureAwait(false);
			return accounts.SingleOrDefault();
		}

		private async Task<IEnumerable<AccountEntity>> GetAccountsAsync(
			CommandDefinition command)
		{
			var oneToOnes =
				await DbConnection
					.QueryAsync<AccountEntity, CurrencyEntity, AccountInCurrencyEntity, CurrencyEntity, OneToOne<AccountEntity, AccountInCurrencyEntity>>(
						command,
						(account, preferredCurrency, inCurrency, currency) =>
						{
							account.PreferredCurrency = preferredCurrency;
							inCurrency.Currency = currency;
							return new(account, inCurrency);
						})
					.ConfigureAwait(false);

			return oneToOnes.GroupBy(oneToOne => oneToOne.First.Id).Select(grouping => AccountEntity.FromGrouping(grouping));
		}
	}
}
