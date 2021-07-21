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
			"INSERT INTO accounts (owner_id, created_by_user_id, modified_by_user_id, name, normalized_name, preferred_currency_id, bic, iban, account_number) VALUES (@OwnerId, @CreatedByUserId, @ModifiedByUserId, @Name, @NormalizedName, @PreferredCurrencyId, @Bic, @Iban, @AccountNumber) RETURNING id";

		private const string _selectSql = @"
SELECT accounts.id,
       accounts.created_at            CreatedAt,
       accounts.owner_id              OwnerId,
       accounts.created_by_user_id    CreatedByUserId,
       accounts.modified_at           ModifiedAt,
       accounts.modified_by_user_id   ModifiedByUserId,
       accounts.name,
       accounts.normalized_name       NormalizedName,
       accounts.preferred_currency_id PreferredCurrencyId,
       accounts.bic,
       accounts.iban,
       accounts.account_number        AccountNumber,
       inCurrency.id,
       inCurrency.created_at          CreatedAt,
       inCurrency.owner_id            OwnerId,
       inCurrency.created_by_user_id  CreatedByUserId,
       inCurrency.modified_at         ModifiedAt,
       inCurrency.modified_by_user_id ModifiedByUserId,
       inCurrency.account_id          AccountId,
       inCurrency.currency_id         CurrencyId,
       currency.id,
       currency.created_at            CreatedAt,
       currency.name,
       currency.normalized_name       NormalizedName,
       currency.numeric_code          NumericCode,
       currency.alphabetic_code       AlphabeticCode,
       currency.minor_unit            MinorUnit,
       currency.official,
       currency.crypto,
       currency.historical,
       currency.active_from           ActiveFrom,
       currency.active_until          ActiveUntil
FROM accounts
         LEFT JOIN accounts_in_currency inCurrency ON accounts.id = inCurrency.account_id
         LEFT JOIN currencies currency ON inCurrency.currency_id = currency.id";

		private const string _deleteSql = "DELETE FROM accounts WHERE id = @Id";

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
			const string sql = _selectSql + " WHERE accounts.id = @id";
			var command = new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken);


			var accountRelationships = await GetAccountsAsync(command).ConfigureAwait(false);
			var relationship = accountRelationships.SingleOrDefaultStruct();

			return relationship is null ? null : Account.Create(relationship.Value);
		}

		public async Task<Account?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " WHERE accounts.normalized_name = @name";
			var command = new CommandDefinition(sql, new { name }, cancellationToken: cancellationToken);

			var accountRelationships = await GetAccountsAsync(command).ConfigureAwait(false);
			var relationship = accountRelationships.SingleOrDefaultStruct();

			return relationship is null ? null : Account.Create(relationship.Value);
		}

		/// <inheritdoc />
		public async Task<Account> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " WHERE accounts.id = @id";
			var command = new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken);

			var accountRelationships = await GetAccountsAsync(command).ConfigureAwait(false);
			var relationship = accountRelationships.Single();
			return Account.Create(relationship);
		}

		public async Task<List<Account>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			var command = new CommandDefinition(_selectSql, cancellationToken: cancellationToken);
			var accountRelationships = await GetAccountsAsync(command).ConfigureAwait(false);
			return accountRelationships.Select(Account.Create).ToList();
		}

		/// <inheritdoc />
		public Task<int> DeleteAsync(Guid id)
		{
			return _dbConnection.ExecuteAsync(_deleteSql, new { id });
		}

		/// <inheritdoc />
		public void Dispose() => _dbConnection.Dispose();

		private Task<IEnumerable<OneToMany<Account, OneToOne<AccountInCurrency, Currency>>>> GetAccountsAsync(
			CommandDefinition command)
		{
			return _dbConnection.QueryOneToManyAsync<Account, AccountInCurrency, Currency>(command);
		}
	}
}
