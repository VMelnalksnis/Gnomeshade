// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Models;
using Gnomeshade.WebApi.Models.Loans;
using Gnomeshade.WebApi.Models.Transactions;

using NodaTime;

namespace Gnomeshade.WebApi.Client;

/// <summary>Provides typed interface for using the transaction API.</summary>
public interface ITransactionClient
{
	/// <summary>Creates a new transaction.</summary>
	/// <param name="transaction">Information for creating the transaction.</param>
	/// <returns>The id of the created transaction.</returns>
	Task<Guid> CreateTransactionAsync(TransactionCreation transaction);

	/// <summary>Creates a new transaction or replaces an existing  one, if one exists with the specified id.</summary>
	/// <param name="id">The id of the transaction.</param>
	/// <param name="transaction">The transaction to create or replace.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task PutTransactionAsync(Guid id, TransactionCreation transaction);

	/// <summary>Gets the specified transaction.</summary>
	/// <param name="id">The id of the transaction to get.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>The transaction with the specified id.</returns>
	Task<Transaction> GetTransactionAsync(Guid id, CancellationToken cancellationToken = default);

	/// <summary>Gets the specified transaction with all details.</summary>
	/// <param name="id">The id of the transaction to get.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>The transaction with the specified id.</returns>
	Task<DetailedTransaction> GetDetailedTransactionAsync(Guid id, CancellationToken cancellationToken = default);

	/// <summary>Gets all transactions within the specified time interval.</summary>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>All transactions within the specified time interval.</returns>
	Task<List<Transaction>> GetTransactionsAsync(CancellationToken cancellationToken = default);

	/// <summary>Gets all transactions within the specified time interval with all details.</summary>
	/// <param name="interval">The interval for which to get the transactions.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>All transactions within the specified time interval.</returns>
	Task<List<DetailedTransaction>> GetDetailedTransactionsAsync(
		Interval interval,
		CancellationToken cancellationToken = default);

	/// <summary>Deletes the specified transaction.</summary>
	/// <param name="id">The id of the transaction to delete.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task DeleteTransactionAsync(Guid id);

	/// <summary>Merges one transaction into another.</summary>
	/// <param name="targetId">The id of the transaction in to which to merge.</param>
	/// <param name="sourceId">The id of the transaction which to merge into the other one.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task MergeTransactionsAsync(Guid targetId, Guid sourceId);

	/// <summary>Merges one transaction into another.</summary>
	/// <param name="targetId">The id of the transaction in to which to merge.</param>
	/// <param name="sourceIds">The ids of the transactions which to merge into the target transactions.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task MergeTransactionsAsync(Guid targetId, IEnumerable<Guid> sourceIds);

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
	/// <param name="transactionId">The id of the transaction for which to get transfers.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>All transfers for the specified transaction.</returns>
	Task<List<Transfer>> GetTransfersAsync(Guid transactionId, CancellationToken cancellationToken = default);

	/// <summary>Gets all transfers.</summary>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>All transfers.</returns>
	Task<List<Transfer>> GetTransfersAsync(CancellationToken cancellationToken = default);

	/// <summary>Gets the specified transfer.</summary>
	/// <param name="id">The id of the transfer to get.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>The transfer with the specified id.</returns>
	Task<Transfer> GetTransferAsync(Guid id, CancellationToken cancellationToken = default);

	/// <summary>Creates a new transfer or replaces an existing one, if one exists with the specified id.</summary>
	/// <param name="id">The id of the transfer.</param>
	/// <param name="transfer">The transfer to create or replace.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task PutTransferAsync(Guid id, TransferCreation transfer);

	/// <summary>Deletes the specified transfer.</summary>
	/// <param name="id">The id of the transfer to delete.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task DeleteTransferAsync(Guid id);

	/// <summary>Gets all purchases.</summary>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>All purchases.</returns>
	Task<List<Purchase>> GetPurchasesAsync(CancellationToken cancellationToken = default);

	/// <summary>Gets all purchases for the specified transaction.</summary>
	/// <param name="transactionId">The id of the transaction for which to get all the purchases.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>All purchases for the specified transaction.</returns>
	Task<List<Purchase>> GetPurchasesAsync(Guid transactionId, CancellationToken cancellationToken = default);

	/// <summary>Gets the specified purchase.</summary>
	/// <param name="id">The id of the purchase to get.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>The purchase with the specified id.</returns>
	Task<Purchase> GetPurchaseAsync(Guid id, CancellationToken cancellationToken = default);

	/// <summary>Creates a new purchase or replaces an existing one, if one exists with the specified id.</summary>
	/// <param name="id">The id of the purchase.</param>
	/// <param name="purchase">The purchase to create or replace.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task PutPurchaseAsync(Guid id, PurchaseCreation purchase);

	/// <summary>Creates a new purchase.</summary>
	/// <param name="purchase">The purchase to create.</param>
	/// <returns>The id of the created purchase.</returns>
	Task<Guid> CreatePurchaseAsync(PurchaseCreation purchase);

	/// <summary>Deletes the specified purchase.</summary>
	/// <param name="id">The id of the purchase to delete.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task DeletePurchaseAsync(Guid id);

	/// <summary>Gets all related transactions for the specified transaction.</summary>
	/// <param name="id">The id of the transaction for which to get related transactions.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>All related transactions for the specified transaction.</returns>
	Task<List<Transaction>> GetRelatedTransactionAsync(Guid id, CancellationToken cancellationToken = default);

	/// <summary>Adds a related transaction.</summary>
	/// <param name="id">The id of the transaction to which to add the relation.</param>
	/// <param name="relatedId">The id of the related transaction.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task AddRelatedTransactionAsync(Guid id, Guid relatedId);

	/// <summary>Removes a related transaction.</summary>
	/// <param name="id">The id of the transaction from which to remove the relation.</param>
	/// <param name="relatedId">The id of the related transaction.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task RemoveRelatedTransactionAsync(Guid id, Guid relatedId);

	/// <summary>Gets all transaction schedules.</summary>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>All transaction schedules.</returns>
	Task<List<TransactionSchedule>> GetTransactionSchedules(CancellationToken cancellationToken = default);

	/// <summary>Gets the specified transaction schedule.</summary>
	/// <param name="id">The id of the transaction schedule to get.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>The transaction schedule with the specified id.</returns>
	Task<TransactionSchedule> GetTransactionSchedule(Guid id, CancellationToken cancellationToken = default);

	/// <summary>Creates a new transaction schedule.</summary>
	/// <param name="schedule">The transaction schedule to create.</param>
	/// <returns>The id of the created transaction schedule.</returns>
	Task<Guid> CreateTransactionSchedule(TransactionScheduleCreation schedule);

	/// <summary>Creates a new transaction schedule or replaces an existing one, if one exists with the specified id.</summary>
	/// <param name="id">The id of the transaction schedule.</param>
	/// <param name="schedule">The transaction schedule to create or replace.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task PutTransactionSchedule(Guid id, TransactionScheduleCreation schedule);

	/// <summary>Deletes the specified transaction schedule.</summary>
	/// <param name="id">The id of the transaction schedule to delete.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task DeleteTransactionSchedule(Guid id);

	/// <summary>Gets all planned transactions within the specified time interval.</summary>
	/// <param name="interval">The interval for which to get the planned transactions.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>All planned transactions within the specified time interval.</returns>
	Task<List<PlannedTransaction>> GetPlannedTransactions(
		Interval interval,
		CancellationToken cancellationToken = default);

	/// <summary>Gets all planned transactions.</summary>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>All planned transactions.</returns>
	Task<List<PlannedTransaction>> GetPlannedTransactions(CancellationToken cancellationToken = default);

	/// <summary>Gets all planned transactions with the specified schedule.</summary>
	/// <param name="scheduleId">The id of the schedule for which to get all planned transactions.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>All planned transactions for the specified schedule.</returns>
	Task<List<PlannedTransaction>> GetPlannedTransactions(
		Guid scheduleId,
		CancellationToken cancellationToken = default);

	/// <summary>Gets the specified planned transaction.</summary>
	/// <param name="id">The id of the planned transaction to get.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>The planned transaction with the specified id.</returns>
	Task<PlannedTransaction> GetPlannedTransaction(Guid id, CancellationToken cancellationToken = default);

	/// <summary>Creates a new planned transaction.</summary>
	/// <param name="transaction">The planned transaction to create.</param>
	/// <returns>The id of the created planned transaction.</returns>
	Task<Guid> CreatePlannedTransaction(PlannedTransactionCreation transaction);

	/// <summary>Creates a new planned transaction or replaces an existing one, if one exists with the specified id.</summary>
	/// <param name="id">The id of the planned transaction.</param>
	/// <param name="transaction">The planned transaction to create or replace.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task PutPlannedTransaction(Guid id, PlannedTransactionCreation transaction);

	/// <summary>Deletes the specified planned transaction.</summary>
	/// <param name="id">The id of the planned transaction to delete.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task DeletePlannedTransaction(Guid id);

	/// <summary>Gets all planned transfers.</summary>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>All planned transfers.</returns>
	Task<List<PlannedTransfer>> GetPlannedTransfers(CancellationToken cancellationToken = default);

	/// <summary>Gets all planned transfers for the specified transaction.</summary>
	/// <param name="transactionId">The id of the transaction for which to get all the planned transfers.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>All planned transfers.</returns>
	Task<List<PlannedTransfer>> GetPlannedTransfers(Guid transactionId, CancellationToken cancellationToken = default);

	/// <summary>Gets the specified planned transfer.</summary>
	/// <param name="id">The id of the planned transfer to get.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>The planned transfer with the specified id.</returns>
	Task<PlannedTransfer> GetPlannedTransfer(Guid id, CancellationToken cancellationToken = default);

	/// <summary>Creates a new planned transfer.</summary>
	/// <param name="transfer">The planned transfer to create.</param>
	/// <returns>The id of the created planned transfer.</returns>
	Task<Guid> CreatePlannedTransfer(PlannedTransferCreation transfer);

	/// <summary>Creates a new planned transfer or replaces an existing one, if one exists with the specified id.</summary>
	/// <param name="id">The id of the planned transfer.</param>
	/// <param name="transfer">The planned transfer to create or replace.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task PutPlannedTransfer(Guid id, PlannedTransferCreation transfer);

	/// <summary>Deletes the specified planned transfer.</summary>
	/// <param name="id">The id of the planned transfer to delete.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task DeletePlannedTransfer(Guid id);

	/// <summary>Gets all planned purchases.</summary>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>All planned purchases.</returns>
	Task<List<PlannedPurchase>> GetPlannedPurchases(CancellationToken cancellationToken = default);

	/// <summary>Gets all planned purchases for the specified transaction.</summary>
	/// <param name="transactionId">The id of the transaction for which to get all the planned purchases.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>All planned purchases.</returns>
	Task<List<PlannedPurchase>> GetPlannedPurchases(Guid transactionId, CancellationToken cancellationToken = default);

	/// <summary>Gets the specified planned purchase.</summary>
	/// <param name="id">The id of the planned purchase to get.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>The planned purchase with the specified id.</returns>
	Task<PlannedPurchase> GetPlannedPurchase(Guid id, CancellationToken cancellationToken = default);

	/// <summary>Creates a new planned purchase.</summary>
	/// <param name="purchase">The planned purchase to create.</param>
	/// <returns>The id of the created planned purchase.</returns>
	Task<Guid> CreatePlannedPurchase(PlannedPurchaseCreation purchase);

	/// <summary>Creates a new planned purchase or replaces an existing one, if one exists with the specified id.</summary>
	/// <param name="id">The id of the planned purchase.</param>
	/// <param name="purchase">The planned purchase to create or replace.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task PutPlannedPurchase(Guid id, PlannedPurchaseCreation purchase);

	/// <summary>Deletes the specified planned purchase.</summary>
	/// <param name="id">The id of the planned purchase to delete.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task DeletePlannedPurchase(Guid id);

	/// <summary>Gets all planned loan payments.</summary>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>All planned loan payments.</returns>
	Task<List<PlannedLoanPayment>> GetPlannedLoanPayments(CancellationToken cancellationToken = default);

	/// <summary>Gets all planned loan payments for the specified transaction.</summary>
	/// <param name="transactionId">The id of the transaction for which to get all the planned loan payments.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>All planned loan payments.</returns>
	Task<List<PlannedLoanPayment>> GetPlannedLoanPayments(
		Guid transactionId,
		CancellationToken cancellationToken = default);

	/// <summary>Gets the specified planned loan payment.</summary>
	/// <param name="id">The id of the planned loan payment to get.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>The planned loan payment with the specified id.</returns>
	Task<PlannedLoanPayment> GetPlannedLoanPayment(Guid id, CancellationToken cancellationToken = default);

	/// <summary>Creates a new planned loan payment.</summary>
	/// <param name="loanPayment">The planned loan payment to create.</param>
	/// <returns>The id of the created planned loan payment.</returns>
	Task<Guid> CreatePlannedLoanPayment(LoanPaymentCreation loanPayment);

	/// <summary>Creates a new planned loan payment or replaces an existing one, if one exists with the specified id.</summary>
	/// <param name="id">The id of the planned loan payment.</param>
	/// <param name="loanPayment">The planned loan payment to create or replace.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task PutPlannedLoanPayment(Guid id, LoanPaymentCreation loanPayment);

	/// <summary>Deletes the specified planned loan payment.</summary>
	/// <param name="id">The id of the planned loan payment to delete.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task DeletePlannedLoanPayment(Guid id);

	/// <summary>Gets all loans.</summary>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>All loans.</returns>
	[Obsolete]
	Task<List<LegacyLoan>> GetLegacyLoans(CancellationToken cancellationToken = default);

	/// <summary>Deletes the specified loan.</summary>
	/// <param name="id">The id of the loan to delete.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	[Obsolete]
	Task DeleteLegacyLoan(Guid id);
}
