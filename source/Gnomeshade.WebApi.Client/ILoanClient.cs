// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Models.Loans;

namespace Gnomeshade.WebApi.Client;

/// <summary>Provides typed interface for using the loan API.</summary>
public interface ILoanClient
{
	/// <summary>Gets the specified loan.</summary>
	/// <param name="id">The id of the loan to get.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>The loan with the specified id.</returns>
	Task<Loan> GetLoanAsync(Guid id, CancellationToken cancellationToken = default);

	/// <summary>Gets all loans.</summary>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>All loans.</returns>
	Task<List<Loan>> GetLoansAsync(CancellationToken cancellationToken = default);

	/// <summary>Creates a new loan.</summary>
	/// <param name="loan">The loan to create.</param>
	/// <returns>The id of the created loan.</returns>
	Task<Guid> CreateLoanAsync(LoanCreation loan);

	/// <summary>Creates a new loan or replaces an existing one, if one exists with the specified id.</summary>
	/// <param name="id">The id of the loan.</param>
	/// <param name="loan">The loan to create or replace.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task PutLoanAsync(Guid id, LoanCreation loan);

	/// <summary>Deletes the loan.</summary>
	/// <param name="id">The id of the loan to delete.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task DeleteLoanAsync(Guid id);

	/// <summary>Gets the specified loan payment.</summary>
	/// <param name="id">The id of the loan payment to get.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>The loan payment with the specified id.</returns>
	Task<LoanPayment> GetLoanPaymentAsync(Guid id, CancellationToken cancellationToken = default);

	/// <summary>Gets all loan payments.</summary>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>All loan payments.</returns>
	Task<List<LoanPayment>> GetLoanPaymentsAsync(CancellationToken cancellationToken = default);

	/// <summary>Gets all loan payments for the specified loan.</summary>
	/// <param name="id">The id of the loan for which to get all the payments.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>All loan payments for the specified loan.</returns>
	Task<List<LoanPayment>> GetLoanPaymentsAsync(Guid id, CancellationToken cancellationToken = default);

	/// <summary>Gets all loan payments for the specified transaction.</summary>
	/// <param name="id">The id of the transaction for which to get all the payments.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>All loan payments for the specified transaction.</returns>
	Task<List<LoanPayment>> GetLoanPaymentsForTransactionAsync(Guid id, CancellationToken cancellationToken = default);

	/// <summary>Creates a new loan payment.</summary>
	/// <param name="loanPayment">The loan payment to create.</param>
	/// <returns>The id of the created loan payment.</returns>
	Task<Guid> CreateLoanPaymentAsync(LoanPaymentCreation loanPayment);

	/// <summary>Creates a new loan payment or replaces an existing one, if one exists with the specified id.</summary>
	/// <param name="id">The id of the loan payment.</param>
	/// <param name="loanPayment">The loan payment to create or replace.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task PutLoanPaymentAsync(Guid id, LoanPaymentCreation loanPayment);

	/// <summary>Deletes the loan payment.</summary>
	/// <param name="id">The id of the loan payment. to delete.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task DeleteLoanPaymentAsync(Guid id);
}
