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

using Tracking.Finance.Data.Models;

namespace Tracking.Finance.Data.Repositories
{
	public sealed class TransactionItemRepository : Repository<TransactionItem>, IModifiableRepository<TransactionItem>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TransactionItemRepository"/> class with a database connection.
		/// </summary>
		public TransactionItemRepository(IDbConnection dbConnection)
			: base(dbConnection)
		{
		}

		/// <inheritdoc/>
		protected override string TableName => "transaction_items";

		/// <inheritdoc/>
		protected override string ColumnNames => "id Id, owner_id OwnerId, transaction_id TransactionId, source_amount SourceAmount, source_account_id SourceAccountId, " +
			"target_amount TargetAmount, target_account_id TargetAccountId, created_at CreatedAt, " +
			"created_by_user_id CreatedByUserId, modified_at ModifiedAt, modified_by_user_id ModifiedByUserId, " +
			"product_id ProductId, amount Amount, bank_reference BankReference, external_reference ExternalReference, " +
			"internal_reference InternalReference, delivery_date DeliveryDate, description Description";

		protected override string InsertSql =>
			$"INSERT INTO {TableName}" +
					"(owner_id, " +
					"transaction_id, " +
					"source_amount, " +
					"source_account_id, " +
					"target_amount, " +
					"target_account_id, " +
					"created_by_user_id, " +
					"modified_by_user_id, " +
					"product_id, " +
					"amount, " +
					"bank_reference, " +
					"external_reference, " +
					"internal_reference, " +
					"delivery_date, " +
					"description) " +
				"VALUES " +
					$"(@{nameof(TransactionItem.OwnerId)}, " +
					$"@{nameof(TransactionItem.TransactionId)}, " +
					$"@{nameof(TransactionItem.SourceAmount)}, " +
					$"@{nameof(TransactionItem.SourceAccountId)}, " +
					$"@{nameof(TransactionItem.TargetAmount)}, " +
					$"@{nameof(TransactionItem.TargetAccountId)}, " +
					$"@{nameof(TransactionItem.CreatedByUserId)}, " +
					$"@{nameof(TransactionItem.ModifiedByUserId)}, " +
					$"@{nameof(TransactionItem.ProductId)}, " +
					$"@{nameof(TransactionItem.Amount)}, " +
					$"@{nameof(TransactionItem.BankReference)}, " +
					$"@{nameof(TransactionItem.ExternalReference)}, " +
					$"@{nameof(TransactionItem.InternalReference)}, " +
					$"@{nameof(TransactionItem.DeliveryDate)}, " +
					$"@{nameof(TransactionItem.Description)}) " +
				"RETURNING id";

		public async Task<List<TransactionItem>> GetAllAsync(Guid transactionId, CancellationToken cancellationToken = default)
		{
			var sql = @$"SELECT {ColumnNames} FROM {TableName} WHERE transaction_id = @TransactionId;";
			var commandDefinition = new CommandDefinition(sql, new { TransactionId = transactionId }, cancellationToken: cancellationToken);

			var entities = await DbConnection.QueryAsync<TransactionItem>(commandDefinition);
			return entities.ToList();
		}

		/// <inheritdoc/>
		public async Task UpdateAsync(TransactionItem entity)
		{
			var sql = $@"
				UPDATE {TableName} 
				SET 
					owner_id = @{nameof(TransactionItem.OwnerId)}, 
					transaction_id = @{nameof(TransactionItem.TransactionId)}, 
					source_amount = @{nameof(TransactionItem.CreatedAt)}, 
					source_account_id = @{nameof(TransactionItem.CreatedAt)}, 
					target_amount = @{nameof(TransactionItem.CreatedAt)}, 
					target_account_id = @{nameof(TransactionItem.CreatedAt)}, 
					modified_at = @{nameof(TransactionItem.ModifiedAt)},
					modified_by_user_id = @{nameof(TransactionItem.ModifiedByUserId)},
				WHERE id = @Id";

			await DbConnection.ExecuteAsync(sql, entity);
		}
	}
}
