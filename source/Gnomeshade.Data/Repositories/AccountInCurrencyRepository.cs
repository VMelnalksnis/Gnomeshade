// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data.Common;
using System.Threading.Tasks;

using Dapper;

using Gnomeshade.Data.Entities;

using Microsoft.Extensions.Logging;

namespace Gnomeshade.Data.Repositories;

/// <summary>Database backed <see cref="AccountInCurrencyRepository"/> repository.</summary>
public sealed class AccountInCurrencyRepository(ILogger<AccountInCurrencyRepository> logger, DbConnection dbConnection)
	: Repository<AccountInCurrencyEntity>(logger, dbConnection)
{
	/// <inheritdoc />
	protected override string DeleteSql => Queries.AccountInCurrency.Delete;

	/// <inheritdoc />
	protected override string InsertSql => Queries.AccountInCurrency.Insert;

	/// <inheritdoc />
	protected override string TableName => "accounts_in_currency a";

	/// <inheritdoc />
	protected override string SelectSql => Queries.AccountInCurrency.Select;

	/// <inheritdoc />
	protected override string UpdateSql => throw new NotImplementedException();

	/// <inheritdoc />
	protected override string FindSql => "a.id = @id";

	protected override string GroupBy => "GROUP BY a.id";

	/// <inheritdoc />
	protected override string NotDeleted => "a.deleted_at IS NULL";

	public Task RestoreDeletedAsync(Guid id, Guid userId, DbTransaction dbTransaction)
	{
		return DbConnection.ExecuteAsync(
			Queries.AccountInCurrency.RestoreDeleted,
			new { id, userId },
			dbTransaction);
	}
}
