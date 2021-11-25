// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data;

using Gnomeshade.Data.Entities;

namespace Gnomeshade.Data.Repositories
{
	/// <summary>
	/// Database backed <see cref="AccountInCurrencyRepository"/> repository.
	/// </summary>
	public sealed class AccountInCurrencyRepository : Repository<AccountInCurrencyEntity>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AccountInCurrencyRepository"/> class with a database connection.
		/// </summary>
		/// <param name="dbConnection">The database connection for executing queries.</param>
		public AccountInCurrencyRepository(IDbConnection dbConnection)
			: base(dbConnection)
		{
		}

		/// <inheritdoc />
		protected override string DeleteSql => "DELETE FROM accounts_in_currency WHERE id = @id AND owner_id = @ownerId;";

		/// <inheritdoc />
		protected override string InsertSql =>
			"INSERT INTO accounts_in_currency (owner_id, created_by_user_id, modified_by_user_id, account_id, currency_id, disabled_at, disabled_by_user_id) VALUES (@OwnerId, @CreatedByUserId, @ModifiedByUserId, @AccountId, @CurrencyId, @DisabledAt, @DisabledByUserId) RETURNING id";

		/// <inheritdoc />
		protected override string SelectSql =>
			"SELECT id, owner_id OwnerId, created_at CreatedAt, created_by_user_id CreatedByUserId, modified_at ModifiedAt, modified_by_user_id ModifiedByUserId, account_id AccountId, currency_id CurrencyId, disabled_at DisabledAt, disabled_by_user_id DisabledByUserId FROM accounts_in_currency";

		/// <inheritdoc />
		protected override string UpdateSql => throw new NotImplementedException();
	}
}
