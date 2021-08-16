// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Models.Transactions;

namespace Gnomeshade.Interfaces.WebApi.Client
{
	/// <summary>
	/// Provides typed interface for using the transaction API.
	/// </summary>
	public interface ITransactionClient
	{
		/// <summary>
		/// Creates a new transaction.
		/// </summary>
		/// <param name="transaction">Information for creating the transaction.</param>
		/// <returns>The id of the created transaction.</returns>
		Task<Guid> CreateTransactionAsync(TransactionCreationModel transaction);

		/// <summary>
		/// Creates a new transaction item, or replaces and existing one if one exists with the specified id.
		/// </summary>
		/// <param name="transactionId">The id of the transaction to which to add a new item.</param>
		/// <param name="item">The transaction item to create or replace.</param>
		/// <returns>The id of the created or replaced transaction item.</returns>
		Task<Guid> PutTransactionItemAsync(Guid transactionId, TransactionItemCreationModel item);

		/// <summary>
		/// Gets the specified transaction.
		/// </summary>
		/// <param name="id">The id of the transaction to get.</param>
		/// <returns>The transaction with the specified id.</returns>
		Task<Transaction> GetTransactionAsync(Guid id);

		/// <summary>
		/// Gets the specified transaction item.
		/// </summary>
		/// <param name="id">The id of the transaction item to get.</param>
		/// <returns>The transaction item with the specified id.</returns>
		Task<TransactionItem> GetTransactionItemAsync(Guid id);

		/// <summary>
		/// Gets all transactions within the specified time period.
		/// </summary>
		/// <param name="from">The time from which to get transactions.</param>
		/// <param name="to">The time until which to get transactions.</param>
		/// <returns>All transactions within the specified time period.</returns>
		Task<List<Transaction>> GetTransactionsAsync(DateTimeOffset? from, DateTimeOffset? to);

		/// <summary>
		/// Deletes the specified transaction.
		/// </summary>
		/// <param name="id">The id of the transaction to delete.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task DeleteTransactionAsync(Guid id);

		/// <summary>
		/// Deletes the specified transaction item.
		/// </summary>
		/// <param name="id">The id of the transaction item to delete.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task DeleteTransactionItemAsync(Guid id);
	}
}
