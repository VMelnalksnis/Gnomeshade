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
	public sealed class TransactionItemRepository : IDisposable, IRepository<TransactionItem>
	{
		private const string _insertSql =
			"INSERT INTO transaction_items (owner_id, transaction_id, source_amount, source_account_id, target_amount, target_account_id, created_by_user_id, modified_by_user_id, product_id, amount, bank_reference, external_reference, internal_reference, description, delivery_date) VALUES (@OwnerId, @TransactionId, @SourceAmount, @SourceAccountId, @TargetAmount, @TargetAccountId, @CreatedByUserId, @ModifiedByUserId, @ProductId, @Amount, @BankReference, @ExternalReference, @InternalReference, @Description, @DeliveryDate) RETURNING id";

		private const string _selectSql =
			"SELECT id, owner_id OwnerId, transaction_id TransactionId, source_amount SourceAmount, source_account_id SourceAccountId, target_amount TargetAmount, target_account_id TargetAccountId, created_by_user_id CreatedByUserId, modified_by_user_id ModifiedByUserId, product_id ProductId, amount, bank_reference BankReference, external_reference ExternalReference, internal_reference InternalReference, description, delivery_date DeliveryDate FROM transaction_items";

		private const string _deleteSql = "DELETE FROM transaction_items WHERE id = @Id";

		private readonly IDbConnection _dbConnection;

		/// <summary>
		/// Initializes a new instance of the <see cref="TransactionItemRepository"/> class with a database connection.
		/// </summary>
		/// <param name="dbConnection">The database connection for executing queries.</param>
		public TransactionItemRepository(IDbConnection dbConnection)
		{
			_dbConnection = dbConnection;
		}

		/// <summary>
		/// Adds a new transaction item.
		/// </summary>
		/// <param name="transactionItem">The transaction item to add.</param>
		/// <returns>The id of the created transaction item.</returns>
		public async Task<Guid> AddAsync(TransactionItem transactionItem)
		{
			return await _dbConnection.QuerySingleAsync<Guid>(_insertSql, transactionItem).ConfigureAwait(false);
		}

		/// <summary>
		/// Adds a new transaction item.
		/// </summary>
		/// <param name="transactionItem">The transaction item to add.</param>
		/// <param name="dbTransaction">The database transaction to use for the query.</param>
		/// <returns>The id of the created transaction item.</returns>
		public async Task<Guid> AddAsync(TransactionItem transactionItem, IDbTransaction dbTransaction)
		{
			var command = new CommandDefinition(_insertSql, transactionItem, dbTransaction);
			return await _dbConnection.QuerySingleAsync<Guid>(command).ConfigureAwait(false);
		}

		/// <summary>
		/// Searches for a transaction item with a specified id.
		/// </summary>
		/// <param name="id">The id to search by.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
		/// <returns>The <see cref="TransactionItem"/> if one exists, otherwise <see langword="null"/>.</returns>
		public async Task<TransactionItem?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " WHERE id = @Id";
			var command = new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken);
			return await _dbConnection.QuerySingleOrDefaultAsync<TransactionItem>(command).ConfigureAwait(false);
		}

		/// <summary>
		/// Gets a transaction item with the specified id.
		/// </summary>
		/// <param name="id">The id to search by.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
		/// <returns>The <see cref="TransactionItem"/> with the specified id.</returns>
		public async Task<TransactionItem> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " WHERE id = @Id";
			var command = new CommandDefinition(_selectSql, sql, cancellationToken: cancellationToken);
			return await _dbConnection.QuerySingleAsync<TransactionItem>(command).ConfigureAwait(false);
		}

		/// <summary>
		/// Gets all transaction items for the specified transaction.
		/// </summary>
		/// <param name="transactionId">The id of the transaction for which to get all the items.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
		/// <returns>A collection of all items linked to the specified transaction.</returns>
		public async Task<List<TransactionItem>> GetAllAsync(
			Guid transactionId,
			CancellationToken cancellationToken = default)
		{
			const string sql = _selectSql + " WHERE transaction_id = @TransactionId;";
			var command = new CommandDefinition(sql, new { TransactionId = transactionId }, cancellationToken: cancellationToken);
			var transactionItems = await _dbConnection.QueryAsync<TransactionItem>(command).ConfigureAwait(false);
			return transactionItems.ToList();
		}

		/// <inheritdoc />
		public async Task<int> DeleteAsync(Guid id)
		{
			return await _dbConnection.ExecuteAsync(_deleteSql, new { id }).ConfigureAwait(false);
		}

		/// <inheritdoc />
		public void Dispose() => _dbConnection.Dispose();
	}
}
