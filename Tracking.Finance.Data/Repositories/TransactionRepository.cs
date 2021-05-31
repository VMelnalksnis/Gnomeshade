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
		protected sealed override string ColumnNames { get; } = "id Id, user_id UserId";

		public TransactionRepository(IDbConnection dbConnection)
			: base(dbConnection)
		{
		}

		/// <inheritdoc/>
		public sealed override async Task<int> AddAsync(Transaction entity)
		{
			var sql = @$"
				INSERT INTO {TableName}
					(user_id)
				VALUES
					(@UserId)
				RETURNING id";

			return await DbConnection.QuerySingleAsync<int>(sql, entity);
		}

		/// <inheritdoc/>
		public async Task<int> UpdateAsync(Transaction entity)
		{
			var sql = $@"
				UPDATE {TableName} 
				SET 
					user_id = @UserId 
				WHERE id = @Id";

			return await DbConnection.ExecuteAsync(sql, entity);
		}
	}
}
