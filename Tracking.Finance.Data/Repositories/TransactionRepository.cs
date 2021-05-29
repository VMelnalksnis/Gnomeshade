using System;
using System.Data;
using System.Threading.Tasks;

using Dapper;

using Tracking.Finance.Data.Models;

namespace Tracking.Finance.Data.Repositories
{
	public sealed class TransactionRepository : Repository<Transaction>, IModifiableRepository<Transaction>
	{
		/// <inheritdoc/>
		protected sealed override string TableName { get; } = "Transactions";

		public TransactionRepository(IDbConnection dbConnection)
			: base(dbConnection)
		{
		}

		/// <inheritdoc/>
		public sealed override async Task<int> AddAsync(Transaction entity)
		{
			throw new NotImplementedException();
			var sql = @$"INSERT INTO {TableName} (UserId) VALUES (@UserId)";

			return await DbConnection.ExecuteAsync(sql, entity);
		}

		/// <inheritdoc/>
		public Task<int> UpdateAsync(Transaction entity) => throw new NotImplementedException();
	}
}
