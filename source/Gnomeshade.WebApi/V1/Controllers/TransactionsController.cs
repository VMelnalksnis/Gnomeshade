// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using Gnomeshade.Data;
using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models;
using Gnomeshade.WebApi.Models.Transactions;
using Gnomeshade.WebApi.OpenApi;
using Gnomeshade.WebApi.V1.Authorization;
using Gnomeshade.WebApi.V1.Transactions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using NodaTime;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.WebApi.V1.Controllers;

/// <summary>CRUD operations on transaction entity.</summary>
public sealed class TransactionsController : CreatableBase<TransactionRepository, TransactionEntity, Transaction, TransactionCreation>
{
	private readonly TransferRepository _transferRepository;
	private readonly PurchaseRepository _purchaseRepository;
	private readonly LoanRepository _loanRepository;
	private readonly TransactionUnitOfWork _unitOfWork;
	private readonly CounterpartyRepository _counterpartyRepository;
	private readonly AccountRepository _accountRepository;

	/// <summary>Initializes a new instance of the <see cref="TransactionsController"/> class.</summary>
	/// <param name="repository">The repository for performing CRUD operations on <see cref="TransactionEntity"/>.</param>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="unitOfWork">Unit of work for managing transactions and all related entities.</param>
	/// <param name="applicationUserContext">Context for getting the current application user.</param>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	/// <param name="transferRepository">Persistence store for <see cref="TransferEntity"/>.</param>
	/// <param name="purchaseRepository">Persistence store for <see cref="PurchaseEntity"/>.</param>
	/// <param name="loanRepository">Persistence store for <see cref="LoanEntity"/>.</param>
	/// <param name="counterpartyRepository">Persistence store for <see cref="CounterpartyEntity"/>.</param>
	/// <param name="accountRepository">Persistence store for <see cref="AccountEntity"/>.</param>
	/// <param name="dbConnection">Database connection for transaction management.</param>
	public TransactionsController(
		TransactionRepository repository,
		ILogger<TransactionsController> logger,
		TransactionUnitOfWork unitOfWork,
		ApplicationUserContext applicationUserContext,
		Mapper mapper,
		TransferRepository transferRepository,
		PurchaseRepository purchaseRepository,
		LoanRepository loanRepository,
		CounterpartyRepository counterpartyRepository,
		AccountRepository accountRepository,
		DbConnection dbConnection)
		: base(applicationUserContext, mapper, logger, repository, dbConnection)
	{
		_unitOfWork = unitOfWork;
		_transferRepository = transferRepository;
		_purchaseRepository = purchaseRepository;
		_loanRepository = loanRepository;
		_counterpartyRepository = counterpartyRepository;
		_accountRepository = accountRepository;
	}

	/// <inheritdoc />
	[NonAction]
	public override Task<List<Transaction>> Get(CancellationToken cancellationToken) =>
		base.Get(cancellationToken);

	/// <inheritdoc cref="ITransactionClient.GetTransactionAsync"/>
	/// <response code="200">Transaction with the specified id exists.</response>
	/// <response code="404">Transaction with the specified id does not exist.</response>
	[ProducesResponseType(typeof(Transaction), Status200OK)]
	public override Task<ActionResult<Transaction>> Get(Guid id, CancellationToken cancellationToken) =>
		base.Get(id, cancellationToken);

	/// <inheritdoc cref="ITransactionClient.GetDetailedTransactionAsync"/>
	/// <response code="200">Transaction with the specified id exists.</response>
	/// <response code="404">Transaction with the specified id does not exist.</response>
	[HttpGet("{id:guid}/Details")]
	[ProducesResponseType(typeof(DetailedTransaction), Status200OK)]
	[ProducesStatus404NotFound]
	public async Task<ActionResult<DetailedTransaction>> GetDetailed(Guid id, CancellationToken cancellationToken)
	{
		var transaction = await Repository.FindByIdAsync(id, ApplicationUser.Id, AccessLevel.Read, cancellationToken);
		if (transaction is null)
		{
			return NotFound();
		}

		var userAccountsInCurrencyIds = await GetUserAccountsInCurrencyIds(cancellationToken);
		var detailedTransaction = await ToDetailed(transaction, userAccountsInCurrencyIds, cancellationToken);
		return Ok(detailedTransaction);
	}

	/// <summary>Gets all transactions.</summary>
	/// <param name="timeRange">A time range for filtering transactions.</param>
	/// <param name="cancellation">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns><see cref="OkObjectResult"/> with the transactions.</returns>
	/// <response code="200">Successfully got all transactions.</response>
	[HttpGet]
	[ProducesResponseType(typeof(List<Transaction>), Status200OK)]
	public async Task<List<Transaction>> GetAll(
		[FromQuery] OptionalTimeRange timeRange,
		CancellationToken cancellation)
	{
		var (fromDate, toDate) = TimeRange.FromOptional(timeRange, SystemClock.Instance.GetCurrentInstant());
		var transactions = (timeRange.From is null && timeRange.To is null) || (timeRange.From == Instant.MinValue && timeRange.To == Instant.MaxValue)
			? await Repository.GetAllAsync(ApplicationUser.Id, cancellation)
			: await Repository.GetAllAsync(fromDate, toDate, ApplicationUser.Id, cancellation);

		return transactions.Select(MapToModel).ToList();
	}

	/// <summary>Gets all transactions.</summary>
	/// <param name="timeRange">A time range for filtering transactions.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns><see cref="OkObjectResult"/> with the transactions.</returns>
	/// <response code="200">Successfully got all transactions.</response>
	[HttpGet("Details")]
	[ProducesResponseType(typeof(List<DetailedTransaction>), Status200OK)]
	public async IAsyncEnumerable<DetailedTransaction> GetAllDetailed(
		[FromQuery] OptionalTimeRange timeRange,
		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var (fromDate, toDate) = TimeRange.FromOptional(timeRange, SystemClock.Instance.GetCurrentInstant());
		var transactions = (timeRange.From is null && timeRange.To is null) || (timeRange.From == Instant.MinValue && timeRange.To == Instant.MaxValue)
			? await Repository.GetAllAsync(ApplicationUser.Id, cancellationToken)
			: await Repository.GetAllAsync(fromDate, toDate, ApplicationUser.Id, cancellationToken);

		var userAccountsInCurrencyIds = await GetUserAccountsInCurrencyIds(cancellationToken);

		foreach (var transaction in transactions)
		{
			yield return await ToDetailed(transaction, userAccountsInCurrencyIds, cancellationToken);
		}
	}

	/// <inheritdoc cref="ITransactionClient.CreateTransactionAsync"/>
	/// <response code="201">Transaction was successfully created.</response>
	public override Task<ActionResult> Post([FromBody] TransactionCreation transaction) =>
		base.Post(transaction);

	/// <inheritdoc cref="ITransactionClient.PutTransactionAsync"/>
	/// <response code="201">Transaction was successfully created.</response>
	/// <response code="204">Transaction was successfully updated.</response>
	public override Task<ActionResult> Put(Guid id, [FromBody] TransactionCreation transaction) =>
		base.Put(id, transaction);

	/// <inheritdoc cref="ITransactionClient.DeleteTransactionAsync"/>
	/// <response code="204">Transaction was successfully deleted.</response>
	/// <response code="404">Transaction with the specified id does not exist.</response>
	// ReSharper disable once RedundantOverriddenMember
	public override Task<StatusCodeResult> Delete(Guid id) =>
		base.Delete(id);

	/// <inheritdoc cref="ITransactionClient.GetTransactionLinksAsync"/>
	/// <response code="200">Successfully got all links.</response>
	[HttpGet("{transactionId:guid}/Links")]
	[ProducesResponseType(typeof(List<Link>), Status200OK)]
	public async Task<List<Link>> GetLinks(Guid transactionId, CancellationToken cancellationToken)
	{
		var links = await Repository.GetAllLinksAsync(transactionId, ApplicationUser.Id, cancellationToken);
		return links.Select(link => Mapper.Map<Link>(link)).ToList();
	}

	/// <inheritdoc cref="ITransactionClient.AddLinkToTransactionAsync"/>
	/// <response code="204">Successfully added link to transaction.</response>
	[HttpPut("{transactionId:guid}/Links/{id:guid}")]
	[ProducesResponseType(Status204NoContent)]
	[ProducesStatus404NotFound]
	public async Task<ActionResult> AddLink(Guid transactionId, Guid id)
	{
		var transaction = await Repository.FindByIdAsync(transactionId, ApplicationUser.Id);
		if (transaction is null)
		{
			return NotFound();
		}

		await Repository.AddLinkAsync(transactionId, id, ApplicationUser.Id);
		return NoContent();
	}

	/// <inheritdoc cref="ITransactionClient.RemoveLinkFromTransactionAsync"/>
	/// <response code="204">Successfully removed link to transaction.</response>
	[HttpDelete("{transactionId:guid}/Links/{id:guid}")]
	[ProducesResponseType(Status204NoContent)]
	[ProducesStatus404NotFound]
	public async Task<ActionResult> RemoveLink(Guid transactionId, Guid id)
	{
		var transaction = await Repository.FindByIdAsync(transactionId, ApplicationUser.Id);
		if (transaction is null)
		{
			return NotFound();
		}

		await Repository.RemoveLinkAsync(transactionId, id, ApplicationUser.Id);
		return NoContent();
	}

	/// <inheritdoc cref="ITransactionClient.GetTransfersAsync(Guid, CancellationToken)"/>
	/// <response code="200">Successfully got all transfers.</response>
	[HttpGet("{transactionId:guid}/Transfers")]
	[ProducesResponseType(typeof(List<Transfer>), Status200OK)]
	public async Task<List<Transfer>> GetTransfers(Guid transactionId, CancellationToken cancellationToken)
	{
		var transfers = await _transferRepository.GetAllAsync(transactionId, ApplicationUser.Id, cancellationToken);
		return transfers.Select(transfer => Mapper.Map<Transfer>(transfer)).ToList();
	}

	/// <inheritdoc cref="ITransactionClient.GetPurchasesAsync(Guid, CancellationToken)"/>
	/// <response code="200">Successfully got all purchases.</response>
	[HttpGet("{transactionId:guid}/Purchases")]
	[ProducesResponseType(typeof(List<Purchase>), Status200OK)]
	public async Task<List<Purchase>> GetPurchases(Guid transactionId, CancellationToken cancellationToken)
	{
		var purchases = await _purchaseRepository.GetAllAsync(transactionId, ApplicationUser.Id, cancellationToken);
		var models = purchases.Select(purchase => Mapper.Map<Purchase>(purchase)).ToList();
		return models;
	}

	/// <inheritdoc cref="ITransactionClient.GetLoansAsync(Guid, CancellationToken)"/>
	/// <response code="200">Successfully got all loans.</response>
	[HttpGet("{transactionId:guid}/Loans")]
	[ProducesResponseType(typeof(List<Loan>), Status200OK)]
	public async Task<List<Loan>> GetLoans(Guid transactionId, CancellationToken cancellationToken)
	{
		var loans = await _loanRepository.GetAllAsync(transactionId, ApplicationUser.Id, cancellationToken);
		return loans.Select(loan => Mapper.Map<Loan>(loan)).ToList();
	}

	/// <inheritdoc cref="ITransactionClient.GetCounterpartyLoansAsync"/>
	/// <response code="200">Successfully got all loans.</response>
	[HttpGet("Loans")]
	[ProducesResponseType(typeof(List<Loan>), Status200OK)]
	public async Task<List<Loan>> GetCounterpartyLoans(Guid counterpartyId, CancellationToken cancellationToken)
	{
		var loans = await _loanRepository.GetAllForCounterpartyAsync(counterpartyId, ApplicationUser.Id, cancellationToken);
		return loans.Select(loan => Mapper.Map<Loan>(loan)).ToList();
	}

	/// <inheritdoc cref="ITransactionClient.MergeTransactionsAsync"/>
	/// <response code="204">Transactions were successfully merged.</response>
	[HttpPost("{targetId:guid}/Merge/{sourceId:guid}")]
	[ProducesResponseType(Status204NoContent)]
	public async Task<ActionResult> Merge(Guid targetId, Guid sourceId)
	{
		await using var dbTransaction = await DbConnection.OpenAndBeginTransaction();
		await Repository.MergeAsync(targetId, sourceId, ApplicationUser.Id, dbTransaction);
		await dbTransaction.CommitAsync();
		return NoContent();
	}

	/// <inheritdoc cref="ITransactionClient.GetRelatedTransactionAsync"/>
	/// <response code="200">Successfully got all related transactions.</response>
	[HttpGet("{id:guid}/Related")]
	[ProducesResponseType(typeof(List<Transaction>), Status200OK)]
	public async Task<IEnumerable<Transaction>> GetRelated(Guid id, CancellationToken cancellationToken)
	{
		var transactions = await Repository.GetRelatedAsync(id, ApplicationUser.Id, cancellationToken);
		return transactions.Select(MapToModel);
	}

	/// <inheritdoc cref="ITransactionClient.AddRelatedTransactionAsync"/>
	/// <response code="204">Related transaction was successfully added.</response>
	[HttpPost("{id:guid}/Related/{relatedId:guid}")]
	[ProducesResponseType(Status204NoContent)]
	public async Task<ActionResult> AddRelated(Guid id, Guid relatedId)
	{
		await Repository.AddRelatedAsync(id, relatedId, ApplicationUser.Id);
		return NoContent();
	}

	/// <inheritdoc cref="ITransactionClient.RemoveRelatedTransactionAsync"/>
	/// <response code="204">Related transaction was successfully removed.</response>
	[HttpDelete("{id:guid}/Related/{relatedId:guid}")]
	[ProducesResponseType(Status204NoContent)]
	public async Task<ActionResult> RemoveRelated(Guid id, Guid relatedId)
	{
		await Repository.RemoveRelatedAsync(id, relatedId, ApplicationUser.Id);
		return NoContent();
	}

	/// <inheritdoc />
	protected override async Task<ActionResult> UpdateExistingAsync(
		Guid id,
		TransactionCreation creation,
		UserEntity user)
	{
		var transaction = Mapper.Map<TransactionEntity>(creation) with
		{
			Id = id,
			ModifiedByUserId = user.Id,
		};

		await _unitOfWork.UpdateAsync(transaction, user);
		return NoContent();
	}

	/// <inheritdoc />
	protected override async Task<ActionResult> CreateNewAsync(Guid id, TransactionCreation creation, UserEntity user)
	{
		var transaction = Mapper.Map<TransactionEntity>(creation) with
		{
			Id = id,
			CreatedByUserId = user.Id,
			ModifiedByUserId = user.Id,
			ImportedAt = creation.ImportedAt ?? (creation.ImportHash is null ? null : SystemClock.Instance.GetCurrentInstant()),
			ReconciledByUserId = creation.ReconciledAt is null ? null : user.Id,
		};

		var transactionId = await _unitOfWork.AddAsync(transaction);
		return CreatedAtAction(nameof(Get), new { id = transactionId }, id);
	}

	private async Task<List<Guid>> GetUserAccountsInCurrencyIds(CancellationToken cancellationToken)
	{
		var userCounterparty = await _counterpartyRepository.GetByIdAsync(ApplicationUser.CounterpartyId, ApplicationUser.Id, cancellationToken);
		var accounts = await _accountRepository.GetAllAsync(ApplicationUser.Id, cancellationToken);
		return accounts
			.Where(account => account.CounterpartyId == userCounterparty.Id)
			.SelectMany(account => account.Currencies)
			.Select(entity => entity.Id)
			.ToList();
	}

	private async Task<DetailedTransaction> ToDetailed(TransactionEntity transaction, List<Guid> userAccountsInCurrencyIds, CancellationToken cancellationToken)
	{
		var id = transaction.Id;
		var transfers = await GetTransfers(id, cancellationToken);
		var transferBalance = transfers.Sum(transfer =>
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

		var purchases = await GetPurchases(id, cancellationToken);
		var purchaseTotal = purchases.Sum(purchase => purchase.Price);
		var loans = await GetLoans(id, cancellationToken);
		var loanTotal = loans.Sum(loan => loan.Amount);

		var links = await GetLinks(id, cancellationToken);

		return Mapper.Map<DetailedTransaction>(transaction) with
		{
			Transfers = transfers,
			TransferBalance = transferBalance,
			Purchases = purchases,
			PurchaseTotal = purchaseTotal,
			Loans = loans,
			LoanTotal = loanTotal,
			Links = links,
		};
	}
}
