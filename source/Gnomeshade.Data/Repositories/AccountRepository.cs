// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Dapper;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Logging;
using Gnomeshade.Data.Repositories.Extensions;

using Microsoft.Extensions.Logging;

using static Gnomeshade.Data.Repositories.AccessLevel;

namespace Gnomeshade.Data.Repositories;

/// <summary>Database backed <see cref="AccountEntity"/> repository.</summary>
public sealed class AccountRepository : NamedRepository<AccountEntity>
{
	/// <summary>Initializes a new instance of the <see cref="AccountRepository"/> class with a database connection.</summary>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public AccountRepository(ILogger<AccountRepository> logger, DbConnection dbConnection)
		: base(logger, dbConnection)
	{
	}

	/// <inheritdoc />
	protected override string DeleteSql => Queries.Account.Delete;

	/// <inheritdoc />
	protected override string InsertSql => Queries.Account.Insert;

	/// <inheritdoc />
	protected override string SelectAllSql => Queries.Account.SelectAll;

	/// <inheritdoc />
	protected override string FindSql => "a.id = @id";

	/// <inheritdoc />
	protected override string UpdateSql => Queries.Account.Update;

	/// <inheritdoc />
	protected override string NotDeleted => "a.deleted_at IS NULL AND aic.deleted_at IS NULL";

	/// <inheritdoc />
	protected override string NameSql => "a.normalized_name = upper(@name)";

	/// <inheritdoc />
	protected override string SelectSql => Queries.Account.Select;

	/// <summary>Finds an account with the specified IBAN.</summary>
	/// <param name="iban">The IBAN for which to search for.</param>
	/// <param name="userId">The id of the user requesting access to the entity.</param>
	/// <param name="dbTransaction">The database transaction to use for the query.</param>
	/// <returns>The account with the IBAN if one exists, otherwise <see langword="null"/>.</returns>
	public Task<AccountEntity?> FindByIbanAsync(string iban, Guid userId, DbTransaction dbTransaction)
	{
		Logger.FindByIbanWithTransaction(iban);
		return FindAsync(new(
			$"{SelectActiveSql} AND a.iban = @iban;",
			new { iban, userId, access = Read.ToParam() },
			dbTransaction));
	}

	/// <summary>Finds an account with the specified BIC.</summary>
	/// <param name="bic">The BIC for which to search for.</param>
	/// <param name="userId">The id of the user requesting access to the entity.</param>
	/// <param name="dbTransaction">The database transaction to use for the query.</param>
	/// <returns>The account with the BIC if one exists, otherwise <see langword="null"/>.</returns>
	public Task<AccountEntity?> FindByBicAsync(string bic, Guid userId, DbTransaction dbTransaction)
	{
		Logger.FindByBicWithTransaction(bic);
		return FindAsync(new(
			$"{SelectActiveSql} AND a.bic = @bic;",
			new { bic, userId, access = Read.ToParam() },
			dbTransaction));
	}

	/// <summary>Gets the sums of all transfers to and from the specified account in all currencies.</summary>
	/// <param name="id">The id of the account for which to get the balance.</param>
	/// <param name="userId">The id of the user requesting access to the entity.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>The sums of all transfers to and from the specified account in all currencies.</returns>
	public Task<IEnumerable<BalanceEntity>> GetBalanceAsync(
		Guid id,
		Guid userId,
		CancellationToken cancellationToken = default)
	{
		Logger.GetBalance(id);
		return DbConnection.QueryAsync<BalanceEntity>(new(
			Queries.Account.Balance,
			new { id, userId },
			cancellationToken: cancellationToken));
	}

	/// <inheritdoc />
	protected override async Task<IEnumerable<AccountEntity>> GetEntitiesAsync(CommandDefinition command)
	{
		var oneToOnes =
			await DbConnection
				.QueryAsync<AccountEntity, AccountInCurrencyEntity, OneToOne<AccountEntity, AccountInCurrencyEntity>>(
					command,
					(account, inCurrency) => new(account, inCurrency))
				.ConfigureAwait(false);

		// ReSharper disable once ConvertClosureToMethodGroup
		return oneToOnes.GroupBy(oneToOne => oneToOne.First.Id).Select(grouping => AccountEntity.FromGrouping(grouping));
	}
}
