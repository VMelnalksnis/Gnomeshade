// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data.Common;

using Gnomeshade.Data.Entities;

namespace Gnomeshade.Data.Repositories;

/// <summary>
/// Database backed <see cref="AccountInCurrencyRepository"/> repository.
/// </summary>
public sealed class AccountInCurrencyRepository : Repository<AccountInCurrencyEntity>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="AccountInCurrencyRepository"/> class with a database connection.
	/// </summary>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public AccountInCurrencyRepository(DbConnection dbConnection)
		: base(dbConnection)
	{
	}

	/// <inheritdoc />
	protected override string DeleteSql => "CALL delete_account_in_currency(@Id, @OwnerId);";

	/// <inheritdoc />
	protected override string InsertSql => Queries.AccountInCurrency.Insert;

	/// <inheritdoc />
	protected override string SelectSql => Queries.AccountInCurrency.Select;

	/// <inheritdoc />
	protected override string UpdateSql => throw new NotImplementedException();

	/// <inheritdoc />
	protected override string FindSql => "WHERE a.deleted_at IS NULL AND a.id = @id";

	/// <inheritdoc />
	protected override string NotDeleted => "a.deleted_at IS NULL";
}
