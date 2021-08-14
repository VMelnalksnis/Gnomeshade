// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data;
using System.Threading.Tasks;

using Dapper;

using Gnomeshade.Data.Entities;

namespace Gnomeshade.Data.Repositories
{
	public sealed class TransactionItemRepository : Repository<TransactionItemEntity>
	{
		private const string _updateSql =
			"UPDATE transaction_items SET modified_at = DEFAULT, modified_by_user_id = @ModifiedByUserId, transaction_id = @TransactionId, source_amount = @SourceAmount, source_account_id = @SourceAccountId, target_amount = @TargetAmount, target_account_id = @TargetAccountId, product_id = @ProductId, amount = @Amount, bank_reference = @BankReference, external_reference = @ExternalReference, internal_reference = @InternalReference, delivery_date = @DeliveryDate, description = @Description WHERE id = @Id RETURNING id;";

		/// <summary>
		/// Initializes a new instance of the <see cref="TransactionItemRepository"/> class with a database connection.
		/// </summary>
		/// <param name="dbConnection">The database connection for executing queries.</param>
		public TransactionItemRepository(IDbConnection dbConnection)
			: base(dbConnection)
		{
		}

		/// <inheritdoc />
		protected override string DeleteSql => "DELETE FROM transaction_items WHERE id = @Id";

		/// <inheritdoc />
		protected override string InsertSql =>
			"INSERT INTO transaction_items (owner_id, transaction_id, source_amount, source_account_id, target_amount, target_account_id, created_by_user_id, modified_by_user_id, product_id, amount, bank_reference, external_reference, internal_reference, description, delivery_date) VALUES (@OwnerId, @TransactionId, @SourceAmount, @SourceAccountId, @TargetAmount, @TargetAccountId, @CreatedByUserId, @ModifiedByUserId, @ProductId, @Amount, @BankReference, @ExternalReference, @InternalReference, @Description, @DeliveryDate) RETURNING id";

		/// <inheritdoc />
		protected override string SelectSql =>
			"SELECT id, owner_id OwnerId, transaction_id TransactionId, source_amount SourceAmount, source_account_id SourceAccountId, target_amount TargetAmount, target_account_id TargetAccountId, created_by_user_id CreatedByUserId, modified_by_user_id ModifiedByUserId, product_id ProductId, amount, bank_reference BankReference, external_reference ExternalReference, internal_reference InternalReference, description, delivery_date DeliveryDate FROM transaction_items";

		public Task<Guid> UpdateAsync(TransactionItemEntity transactionItem)
		{
			var command = new CommandDefinition(_updateSql, transactionItem);
			return DbConnection.QuerySingleAsync<Guid>(command);
		}
	}
}
