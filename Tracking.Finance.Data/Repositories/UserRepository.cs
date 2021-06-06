// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Data;
using System.Threading.Tasks;

using Dapper;

using Tracking.Finance.Data.Models;

namespace Tracking.Finance.Data.Repositories
{
	public sealed class UserRepository : Repository<User>
	{
		/// <inheritdoc/>
		protected sealed override string TableName { get; } = "users";

		/// <inheritdoc/>
		protected sealed override string ColumnNames { get; } = "id Id, identity_user_id IdentityUserId, counterparty_id CounterpartyId";

		public UserRepository(IDbConnection dbConnection)
			: base(dbConnection)
		{
		}

		/// <inheritdoc/>
		public sealed override async Task<int> AddAsync(User entity)
		{
			var sql =
				$"INSERT INTO {TableName} (identity_user_id, counterparty_id) VALUES " +
				$"(@{nameof(User.@IdentityUserId)}, @{nameof(User.CounterpartyId)}) " +
				"RETURNING id";

			return await DbConnection.QuerySingleAsync<int>(sql, entity);
		}
	}
}
