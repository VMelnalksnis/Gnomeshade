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
	/// <summary>
	/// Database backed <see cref="AccountEntity"/> repository.
	/// </summary>
	public sealed class AccountRepository : NamedRepository<AccountEntity>
	{
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
			"LEFT JOIN currencies c ON aic.currency_id = c.id ";

		/// <summary>
		/// Initializes a new instance of the <see cref="AccountRepository"/> class with a database connection.
		/// </summary>
		/// <param name="dbConnection">The database connection for executing queries.</param>
		public AccountRepository(IDbConnection dbConnection)
			: base(dbConnection)
		{
		}

		/// <inheritdoc />
		protected override string DeleteSql => "DELETE FROM accounts WHERE id = @id AND owner_id = @ownerId;";

		/// <inheritdoc />
		protected override string InsertSql =>
			"INSERT INTO accounts (owner_id, created_by_user_id, modified_by_user_id, name, normalized_name,counterparty_id, preferred_currency_id, disabled_at, disabled_by_user_id, bic, iban, account_number) VALUES (@OwnerId, @CreatedByUserId, @ModifiedByUserId, @Name, @NormalizedName, @CounterpartyId, @PreferredCurrencyId, @DisabledAt, @DisabledByUserId, @Bic, @Iban, @AccountNumber) RETURNING id;";

		/// <inheritdoc />
		protected override string SelectSql => _selectSql;

		/// <inheritdoc />
		protected override string FindSql => "WHERE a.id = @id AND a.owner_id = @ownerId;";

		/// <inheritdoc />
		protected override string UpdateSql => throw new NotImplementedException();

		/// <inheritdoc />
		protected override string NameSql => "WHERE a.normalized_name = @name AND a.owner_id = @ownerId;";

		/// <summary>
		/// Finds an account with the specified IBAN.
		/// </summary>
		/// <param name="iban">The IBAN for which to search for.</param>
		/// <param name="ownerId">The id of the owner of the entity.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
		/// <returns>The account with the IBAN if one exists, otherwise <see langword="null"/>.</returns>
		public Task<AccountEntity?> FindByIbanAsync(string iban, Guid ownerId, CancellationToken cancellationToken = default)
		{
			const string sql = $"{_selectSql} WHERE a.iban = @iban AND a.owner_id = @ownerId;";
			var command = new CommandDefinition(sql, new { iban, ownerId }, cancellationToken: cancellationToken);
			return FindAsync(command);
		}

		/// <summary>
		/// Finds an account with the specified BIC.
		/// </summary>
		/// <param name="bic">The BIC for which to search for.</param>
		/// <param name="ownerId">The id of the owner of the entity.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
		/// <returns>The account with the BIC if one exists, otherwise <see langword="null"/>.</returns>
		public Task<AccountEntity?> FindByBicAsync(string bic, Guid ownerId, CancellationToken cancellationToken = default)
		{
			const string sql = $"{_selectSql} WHERE a.bic = @bic AND a.owner_id = @ownerId;";
			var command = new CommandDefinition(sql, new { bic, ownerId }, cancellationToken: cancellationToken);
			return FindAsync(command);
		}

		/// <summary>
		/// Gets all accounts accounts that have not been disabled, with currencies which also have not been disabled.
		/// </summary>
		/// <param name="ownerId">The id of the owner of the entity.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
		/// <returns>A collection of all active accounts.</returns>
		public Task<IEnumerable<AccountEntity>> GetAllActiveAsync(Guid ownerId, CancellationToken cancellationToken = default)
		{
			const string sql = $"{_selectSql} WHERE a.disabled_at IS NULL AND aic.disabled_at IS NULL AND a.owner_id = @ownerId ORDER BY a.created_at;";
			var command = new CommandDefinition(sql, new { ownerId }, cancellationToken: cancellationToken);
			return GetEntitiesAsync(command);
		}

		/// <inheritdoc />
		protected override async Task<IEnumerable<AccountEntity>> GetEntitiesAsync(CommandDefinition command)
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

			// ReSharper disable once ConvertClosureToMethodGroup
			return oneToOnes.GroupBy(oneToOne => oneToOne.First.Id).Select(grouping => AccountEntity.FromGrouping(grouping));
		}
	}
}
