// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU General Public License 3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Data;
using System.Threading.Tasks;

using Dapper;

using Tracking.Finance.Data.Models;

namespace Tracking.Finance.Data.Repositories
{
	public sealed class TransactionRepository : Repository<Transaction>, IModifiableRepository<Transaction>
	{
		/// <inheritdoc/>
		protected sealed override string TableName { get; } = "public.\"transactions\"";

		/// <inheritdoc/>
		protected sealed override string ColumnNames { get; } =
			"id Id, user_id UserId, created_at CreatedAt, created_by_user_id CreatedByUserId, " +
			"modified_at ModifiedAt, modified_by_user_id ModifiedByUserId, date Date, " +
			"description Description, generated \"Generated\", validated Validated, completed Completed";

		public TransactionRepository(IDbConnection dbConnection)
			: base(dbConnection)
		{
		}

		/// <inheritdoc/>
		public sealed override async Task<int> AddAsync(Transaction entity)
		{
			var sql = @$"
				INSERT INTO {TableName}
					(user_id, created_at, created_by_user_id, modified_at, modified_by_user_id, date, description, generated, validated, completed)
				VALUES
					(@UserId, @CreatedAt, @CreatedByUserId, @ModifiedAt, @ModifiedByUserId, @Date, @Description, @Generated, @Validated, @Completed)
				RETURNING id";

			return await DbConnection.QuerySingleAsync<int>(sql, entity);
		}

		/// <inheritdoc/>
		public async Task UpdateAsync(Transaction entity)
		{
			var sql = $@"
				UPDATE {TableName} 
				SET 
					user_id = @UserId, 
					created_at = @CreatedAt, 
					created_by_user_id = @CreatedByUserId,
					modified_at = @ModifiedAt,
					modified_by_user_id = @ModifiedByUserId,
					date = @Date,
					description = @Description,
					generated = @Generated,
					validated = @Validated,
					completed = @Completed
				WHERE id = @Id";

			await DbConnection.ExecuteAsync(sql, entity);
		}
	}
}
