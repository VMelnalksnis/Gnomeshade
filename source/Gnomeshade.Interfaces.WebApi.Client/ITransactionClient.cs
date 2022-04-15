// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Models;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;

using NodaTime;

namespace Gnomeshade.Interfaces.WebApi.Client;

/// <summary>Provides typed interface for using the transaction API.</summary>
public interface ITransactionClient
{
	/// <summary>Creates a new transaction.</summary>
	/// <param name="transaction">Information for creating the transaction.</param>
	/// <returns>The id of the created transaction.</returns>
	Task<Guid> CreateTransactionAsync(TransactionCreationModel transaction);

	/// <summary>Creates a new transaction or replaces an existing  one, if one exists with the specified id.</summary>
	/// <param name="id">The id of the transaction.</param>
	/// <param name="transaction">The transaction to create or replace.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task PutTransactionAsync(Guid id, TransactionCreationModel transaction);

	/// <summary>Gets the specified transaction.</summary>
	/// <param name="id">The id of the transaction to get.</param>
	/// <returns>The transaction with the specified id.</returns>
	Task<Transaction> GetTransactionAsync(Guid id);

	/// <summary>Gets all transactions within the specified time period.</summary>
	/// <param name="from">The time from which to get transactions.</param>
	/// <param name="to">The time until which to get transactions.</param>
	/// <returns>All transactions within the specified time period.</returns>
	Task<List<Transaction>> GetTransactionsAsync(Instant? from, Instant? to);

	/// <summary>Deletes the specified transaction.</summary>
	/// <param name="id">The id of the transaction to delete.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task DeleteTransactionAsync(Guid id);

	/// <summary>Gets all links for the specified transaction.</summary>
	/// <param name="transactionId">The id of the transaction for which to get the links.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>All links for the specified transaction.</returns>
	Task<List<Link>> GetTransactionLinksAsync(Guid transactionId, CancellationToken cancellationToken = default);

	/// <summary>Adds the specified link to a transaction.</summary>
	/// <param name="transactionId">The id of the transaction to which to add the link.</param>
	/// <param name="linkId">The id of the link to add to the transaction.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task AddLinkToTransactionAsync(Guid transactionId, Guid linkId);

	/// <summary>Removes the specified link from a transaction.</summary>
	/// <param name="transactionId">The id of the transaction from which to remove the link.</param>
	/// <param name="linkId">The id of the link to remove from the transaction.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task RemoveLinkFromTransactionAsync(Guid transactionId, Guid linkId);

	/// <summary>Gets all transfers for the specified transaction.</summary>
	/// <param name="transactionId">The id of the transaction for which to get all the transfers.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>All transfers for the specified transaction.</returns>
	Task<List<Transfer>> GetTransfersAsync(Guid transactionId, CancellationToken cancellationToken = default);

	/// <summary>Gets the specified transfer.</summary>
	/// <param name="transactionId">The id of transaction of which the transfer is a part of.</param>
	/// <param name="id">The id of the transfer to get.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>The transfer with the specified id.</returns>
	Task<Transfer> GetTransferAsync(Guid transactionId, Guid id, CancellationToken cancellationToken = default);

	/// <summary>Creates a new transfer or replaces an existing one, if one exists with the specified id.</summary>
	/// <param name="transactionId">The id of the transaction to which to add a new transfer.</param>
	/// <param name="id">The id of the transfer.</param>
	/// <param name="transfer">The transfer to create or replace.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task PutTransferAsync(Guid transactionId, Guid id, TransferCreation transfer);

	/// <summary>Deletes the specified transfer.</summary>
	/// <param name="transactionId">The id of the transaction of the transfer.</param>
	/// <param name="id">The id of the transfer to delete.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task DeleteTransferAsync(Guid transactionId, Guid id);

	/// <summary>Gets all purchases for the specified transaction.</summary>
	/// <param name="transactionId">The id of the transaction for which to get all the purchases.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>All purchases for the specified transaction.</returns>
	Task<List<Purchase>> GetPurchasesAsync(Guid transactionId, CancellationToken cancellationToken = default);

	/// <summary>Gets the specified purchase.</summary>
	/// <param name="transactionId">The id of transaction of which the purchase is a part of.</param>
	/// <param name="id">The id of the purchase to get.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>The purchase with the specified id.</returns>
	Task<Purchase> GetPurchaseAsync(Guid transactionId, Guid id, CancellationToken cancellationToken = default);

	/// <summary>Creates a new purchase or replaces an existing one, if one exists with the specified id.</summary>
	/// <param name="transactionId">The id of the transaction to which to add a new purchase.</param>
	/// <param name="id">The id of the purchase.</param>
	/// <param name="purchase">The purchase to create or replace.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task PutPurchaseAsync(Guid transactionId, Guid id, PurchaseCreation purchase);

	/// <summary>Deletes the specified purchase.</summary>
	/// <param name="transactionId">The id of the transaction of the purchase.</param>
	/// <param name="id">The id of the purchase to delete.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task DeletePurchaseAsync(Guid transactionId, Guid id);
}
