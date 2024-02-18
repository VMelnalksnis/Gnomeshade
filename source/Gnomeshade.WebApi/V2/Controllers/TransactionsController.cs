// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models;
using Gnomeshade.WebApi.Models.Loans;
using Gnomeshade.WebApi.Models.Transactions;
using Gnomeshade.WebApi.OpenApi;
using Gnomeshade.WebApi.V1;

using Microsoft.AspNetCore.Mvc;

using NodaTime;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.WebApi.V2.Controllers;

/// <summary>CRUD operations on transaction entity.</summary>
public sealed class TransactionsController : FinanceControllerBase<LoanPaymentEntity, LoanPayment>
{
	private readonly TransactionRepository _repository;
	private readonly CounterpartyRepository _counterpartyRepository;
	private readonly AccountRepository _accountRepository;
	private readonly LoanPaymentRepository _paymentRepository;

	/// <summary>Initializes a new instance of the <see cref="TransactionsController"/> class.</summary>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	/// <param name="repository">The repository for performing CRUD operations on <see cref="TransactionEntity"/>.</param>
	/// <param name="paymentRepository">The repository for performing CRUD operations on <see cref="LoanPaymentEntity"/>.</param>
	/// <param name="accountRepository">The repository for performing CRUD operations on <see cref="AccountEntity"/>.</param>
	/// <param name="counterpartyRepository">The repository for performing CRUD operations on <see cref="CounterpartyEntity"/>.</param>
	public TransactionsController(
		TransactionRepository repository,
		CounterpartyRepository counterpartyRepository,
		AccountRepository accountRepository,
		LoanPaymentRepository paymentRepository,
		Mapper mapper)
		: base(mapper)
	{
		_repository = repository;
		_counterpartyRepository = counterpartyRepository;
		_accountRepository = accountRepository;
		_paymentRepository = paymentRepository;
	}

	/// <inheritdoc cref="ITransactionClient.GetDetailedTransactionAsync"/>
	/// <response code="200">Transaction with the specified id exists.</response>
	/// <response code="404">Transaction with the specified id does not exist.</response>
	[HttpGet("{id:guid}/Details")]
	[ProducesResponseType<DetailedTransaction>(Status200OK)]
	[ProducesStatus404NotFound]
	public async Task<ActionResult<DetailedTransaction>> GetDetailed(Guid id, CancellationToken cancellationToken)
	{
		var transaction = await _repository.FindDetailed2Async(id, ApplicationUser.Id, cancellationToken);
		if (transaction is null)
		{
			return NotFound();
		}

		var userAccountsInCurrencyIds = await GetUserAccountsInCurrencyIds(cancellationToken);
		var detailedTransaction = ToDetailed(transaction, userAccountsInCurrencyIds);
		return Ok(detailedTransaction);
	}

	/// <summary>Gets all transactions.</summary>
	/// <param name="timeRange">A time range for filtering transactions.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns><see cref="OkObjectResult"/> with the transactions.</returns>
	/// <response code="200">Successfully got all transactions.</response>
	[HttpGet("Details")]
	[ProducesResponseType<List<DetailedTransaction>>(Status200OK)]
	public async IAsyncEnumerable<DetailedTransaction> GetAllDetailed(
		[FromQuery] V1.Transactions.OptionalTimeRange timeRange,
		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var (fromDate, toDate) = V1.Transactions.TimeRange.FromOptional(timeRange, SystemClock.Instance.GetCurrentInstant());
		var transactions = await _repository.GetAllDetailed2Async(fromDate, toDate, ApplicationUser.Id, cancellationToken);
		var userAccountsInCurrencyIds = await GetUserAccountsInCurrencyIds(cancellationToken);

		foreach (var transaction in transactions)
		{
			yield return ToDetailed(transaction, userAccountsInCurrencyIds);
		}
	}

	/// <inheritdoc cref="ILoanClient.GetLoanPaymentsForTransactionAsync"/>
	/// <response code="200">Successfully got all loan payments.</response>
	[HttpGet("{id:guid}/LoanPayments")]
	[ProducesResponseType<List<LoanPayment>>(Status200OK)]
	public async Task<List<LoanPayment>> GetLoanPayments(Guid id, CancellationToken cancellationToken)
	{
		var entities = await _paymentRepository.GetAllForTransactionAsync(id, ApplicationUser.Id, cancellationToken);
		return entities.Select(MapToModel).ToList();
	}

	private async Task<List<Guid>> GetUserAccountsInCurrencyIds(CancellationToken cancellationToken)
	{
		var userCounterparty = await _counterpartyRepository.GetByIdAsync(ApplicationUser.CounterpartyId, ApplicationUser.Id, cancellationToken);
		var accounts = await _accountRepository.GetAsync(ApplicationUser.Id, cancellationToken);
		return accounts
			.Where(account => account.CounterpartyId == userCounterparty.Id)
			.SelectMany(account => account.Currencies)
			.Select(entity => entity.Id)
			.ToList();
	}

	private DetailedTransaction ToDetailed(
		DetailedTransaction2Entity transaction,
		List<Guid> userAccountsInCurrencyIds)
	{
		var transferBalance = transaction.Transfers.Sum(transfer =>
		{
			var sourceIsUser = userAccountsInCurrencyIds.Contains(transfer.SourceAccountId);
			var targetIsUser = userAccountsInCurrencyIds.Contains(transfer.TargetAccountId);
			return (sourceIsUser, targetIsUser) switch
			{
				(true, true) => 0,
				(true, false) => -transfer.SourceAmount,
				(false, true) => transfer.TargetAmount,
				_ => 0,
			};
		});

		var purchaseTotal = transaction.Purchases.Sum(purchase => purchase.Price);
		var loanTotal = transaction.LoanPayments.Sum(loan => loan.Amount + loan.Interest);

		return Mapper.Map<DetailedTransaction>(transaction) with
		{
			Transfers = transaction.Transfers.Select(transfer => Mapper.Map<Transfer>(transfer)).ToList(),
			TransferBalance = transferBalance,
			Purchases = transaction.Purchases.Select(purchase => Mapper.Map<Purchase>(purchase)).ToList(),
			PurchaseTotal = purchaseTotal,
			LoanPayments = transaction.LoanPayments.Select(loan => Mapper.Map<LoanPayment>(loan)).ToList(),
			LoanTotal = loanTotal,
			Links = transaction.Links.Select(link => Mapper.Map<Link>(link)).ToList(),
		};
	}
}
