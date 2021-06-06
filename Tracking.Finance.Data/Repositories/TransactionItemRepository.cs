// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Dapper;

using Tracking.Finance.Data.Models;

namespace Tracking.Finance.Data.Repositories
{
	public sealed class TransactionItemRepository : Repository<TransactionItem>, IModifiableRepository<TransactionItem>
	{
		/// <inheritdoc/>
		protected sealed override string TableName { get; } = "transaction_items";

		/// <inheritdoc/>
		protected sealed override string ColumnNames { get; } =
			"id Id, user_id UserId, transaction_id TransactionId, source_amount SourceAmount, source_account_id SourceAccountId, " +
			"target_amount TargetAmount, target_account_id TargetAccountId, created_at CreatedAt, " +
			"created_by_user_id CreatedByUserId, modified_at ModifiedAt, modified_by_user_id ModifiedByUserId, " +
			"product_id ProductId, amount Amount, bank_reference BankReference, external_reference ExternalReference, " +
			"internal_reference InternalReference, delivery_date DeliveryDate, description Description";

		public TransactionItemRepository(IDbConnection dbConnection)
			: base(dbConnection)
		{
		}

		/// <inheritdoc/>
		public sealed async override Task<int> AddAsync(TransactionItem entity)
		{
			var sql =
				$"INSERT INTO {TableName}" +
					"(user_id, " +
					"transaction_id, " +
					"source_amount, " +
					"source_account_id, " +
					"target_amount, " +
					"target_account_id, " +
					"created_at, " +
					"created_by_user_id, " +
					"modified_at, " +
					"modified_by_user_id, " +
					"product_id, " +
					"amount, " +
					"bank_reference, " +
					"external_reference, " +
					"internal_reference, " +
					"delivery_date, " +
					"description) " +
				"VALUES " +
					$"(@{nameof(TransactionItem.UserId)}, " +
					$"@{nameof(TransactionItem.TransactionId)}, " +
					$"@{nameof(TransactionItem.SourceAmount)}, " +
					$"@{nameof(TransactionItem.SourceAccountId)}, " +
					$"@{nameof(TransactionItem.TargetAmount)}, " +
					$"@{nameof(TransactionItem.TargetAccountId)}, " +
					$"@{nameof(TransactionItem.CreatedAt)}, " +
					$"@{nameof(TransactionItem.CreatedByUserId)}, " +
					$"@{nameof(TransactionItem.ModifiedAt)}, " +
					$"@{nameof(TransactionItem.ModifiedByUserId)}, " +
					$"@{nameof(TransactionItem.ProductId)}, " +
					$"@{nameof(TransactionItem.Amount)}, " +
					$"@{nameof(TransactionItem.BankReference)}, " +
					$"@{nameof(TransactionItem.ExternalReference)}, " +
					$"@{nameof(TransactionItem.InternalReference)}, " +
					$"@{nameof(TransactionItem.DeliveryDate)}, " +
					$"@{nameof(TransactionItem.Description)}) " +
				"RETURNING id";

			return await DbConnection.QuerySingleAsync<int>(sql, entity);
		}

		public async Task<List<TransactionItem>> GetAllAsync(int transationId, CancellationToken cancellationToken = default)
		{
			var sql = @$"SELECT {ColumnNames} FROM {TableName} WHERE transaction_id = @TransactionId;";
			var commandDefinition = new CommandDefinition(sql, new { TransactionId = transationId }, cancellationToken: cancellationToken);

			var entities = await DbConnection.QueryAsync<TransactionItem>(commandDefinition);
			return entities.ToList();
		}

		/// <inheritdoc/>
		public async Task UpdateAsync(TransactionItem entity)
		{
			var sql = $@"
				UPDATE {TableName} 
				SET 
					user_id = @{nameof(TransactionItem.UserId)}, 
					user_id = @{nameof(TransactionItem.TransactionId)}, 
					source_amount = @{nameof(TransactionItem.CreatedAt)}, 
					source_account_id = @{nameof(TransactionItem.CreatedAt)}, 
					target_amount = @{nameof(TransactionItem.CreatedAt)}, 
					target_account_id = @{nameof(TransactionItem.CreatedAt)}, 
					created_at = @{nameof(TransactionItem.CreatedAt)}, 
					created_by_user_id = @{nameof(TransactionItem.CreatedByUserId)},
					modified_at = @{nameof(TransactionItem.ModifiedAt)},
					modified_by_user_id = @{nameof(TransactionItem.ModifiedByUserId)},
				WHERE id = @Id";

			await DbConnection.ExecuteAsync(sql, entity);
		}
	}
}
