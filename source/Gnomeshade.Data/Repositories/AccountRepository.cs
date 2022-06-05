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

namespace Gnomeshade.Data.Repositories;

/// <summary>Database backed <see cref="AccountEntity"/> repository.</summary>
public sealed class AccountRepository : NamedRepository<AccountEntity>
{
	/// <summary>Initializes a new instance of the <see cref="AccountRepository"/> class with a database connection.</summary>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public AccountRepository(IDbConnection dbConnection)
		: base(dbConnection)
	{
	}

	/// <inheritdoc />
	protected override string DeleteSql => Queries.Account.Delete;

	/// <inheritdoc />
	protected override string InsertSql => Queries.Account.Insert;

	/// <inheritdoc />
	protected override string SelectSql => Queries.Account.Select;

	/// <inheritdoc />
	protected override string FindSql => "WHERE a.id = @id";

	/// <inheritdoc />
	protected override string UpdateSql => Queries.Account.Update;

	/// <inheritdoc />
	protected override string NameSql => "WHERE a.normalized_name = @name";

	/// <summary>Finds an account with the specified IBAN.</summary>
	/// <param name="iban">The IBAN for which to search for.</param>
	/// <param name="ownerId">The id of the owner of the entity.</param>
	/// <param name="dbTransaction">The database transaction to use for the query.</param>
	/// <returns>The account with the IBAN if one exists, otherwise <see langword="null"/>.</returns>
	public Task<AccountEntity?> FindByIbanAsync(string iban, Guid ownerId, IDbTransaction dbTransaction)
	{
		var sql = $"{SelectSql} WHERE a.iban = @iban AND {AccessSql};";
		var command = new CommandDefinition(sql, new { iban, ownerId }, dbTransaction);
		return FindAsync(command);
	}

	/// <summary>Finds an account with the specified BIC.</summary>
	/// <param name="bic">The BIC for which to search for.</param>
	/// <param name="ownerId">The id of the owner of the entity.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>The account with the BIC if one exists, otherwise <see langword="null"/>.</returns>
	public Task<AccountEntity?> FindByBicAsync(string bic, Guid ownerId, CancellationToken cancellationToken = default)
	{
		var sql = $"{SelectSql} WHERE a.bic = @bic AND {AccessSql};";
		var command = new CommandDefinition(sql, new { bic, ownerId }, cancellationToken: cancellationToken);
		return FindAsync(command);
	}

	/// <summary>Gets all accounts accounts that have not been disabled, with currencies which also have not been disabled.</summary>
	/// <param name="ownerId">The id of the owner of the entity.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>A collection of all active accounts.</returns>
	public Task<IEnumerable<AccountEntity>> GetAllActiveAsync(Guid ownerId, CancellationToken cancellationToken = default)
	{
		var sql = $"{SelectSql} WHERE a.disabled_at IS NULL AND aic.disabled_at IS NULL AND {AccessSql} ORDER BY a.created_at;";
		var command = new CommandDefinition(sql, new { ownerId }, cancellationToken: cancellationToken);
		return GetEntitiesAsync(command);
	}

	/// <summary>Gets the sums of all transfers to and from the specified account in all currencies.</summary>
	/// <param name="id">The id of the account for which to get the balance.</param>
	/// <param name="ownerId">The id of the owner of the account.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>The sums of all transfers to and from the specified account in all currencies.</returns>
	public Task<IEnumerable<BalanceEntity>> GetBalanceAsync(
		Guid id,
		Guid ownerId,
		CancellationToken cancellationToken = default)
	{
		var command = new CommandDefinition(Queries.Account.Balance, new { id, ownerId }, cancellationToken: cancellationToken);
		return DbConnection.QueryAsync<BalanceEntity>(command);
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
