﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
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
using Gnomeshade.WebApi.V1_0.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using NodaTime;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.WebApi.V1_0.Transactions;

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
		AccountRepository accountRepository)
		: base(applicationUserContext, mapper, logger, repository)
	{
		_unitOfWork = unitOfWork;
		_transferRepository = transferRepository;
		_purchaseRepository = purchaseRepository;
		_loanRepository = loanRepository;
		_counterpartyRepository = counterpartyRepository;
		_accountRepository = accountRepository;
	}

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
		var transaction = await Repository.FindByIdAsync(id, cancellationToken);
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
		var transactions = await Repository.GetAllAsync(fromDate, toDate, ApplicationUser.Id, cancellation);
		return transactions.Select(MapToModel).ToList();
	}

	/// <summary>Gets all transactions.</summary>
	/// <param name="timeRange">A time range for filtering transactions.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns><see cref="OkObjectResult"/> with the transactions.</returns>
	/// <response code="200">Successfully got all transactions.</response>
	[HttpGet("Details")]
	[ProducesResponseType(typeof(List<Transaction>), Status200OK)]
	public async Task<List<DetailedTransaction>> GetAllDetailed(
		[FromQuery] OptionalTimeRange timeRange,
		CancellationToken cancellationToken)
	{
		var (fromDate, toDate) = TimeRange.FromOptional(timeRange, SystemClock.Instance.GetCurrentInstant());
		var transactions = await Repository.GetAllAsync(fromDate, toDate, ApplicationUser.Id, cancellationToken);

		var userAccountsInCurrencyIds = await GetUserAccountsInCurrencyIds(cancellationToken);
		var detailedTransactions = transactions
			.Select(async transaction => await ToDetailed(transaction, userAccountsInCurrencyIds, cancellationToken))
			.Select(task => task.Result)
			.ToList();

		return detailedTransactions;
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

	/// <inheritdoc cref="ITransactionClient.GetTransfersAsync"/>
	/// <response code="200">Successfully got all transfers.</response>
	[HttpGet("{transactionId:guid}/Transfers")]
	[ProducesResponseType(typeof(List<Transfer>), Status200OK)]
	public async Task<List<Transfer>> GetTransfers(
		Guid transactionId,
		CancellationToken cancellationToken)
	{
		var transfers = await _transferRepository.GetAllAsync(transactionId, ApplicationUser.Id, cancellationToken);
		return transfers.Select(transfer => Mapper.Map<Transfer>(transfer)).ToList();
	}

	/// <inheritdoc cref="ITransactionClient.GetTransferAsync"/>
	/// <response code="200">Successfully got the transfer with the specified id.</response>
	/// <response code="404">Transfer with the specified id does not exist.</response>
	[HttpGet("{transactionId:guid}/Transfers/{id:guid}")]
	[ProducesResponseType(typeof(Transfer), Status200OK)]
	[ProducesStatus404NotFound]
	public async Task<ActionResult<Transfer>> GetTransfer(
		Guid transactionId,
		Guid id,
		CancellationToken cancellationToken)
	{
		var transfer = await _transferRepository.FindByIdAsync(transactionId, id, ApplicationUser.Id, cancellationToken);
		if (transfer is null)
		{
			return NotFound();
		}

		var model = Mapper.Map<Transfer>(transfer);
		return Ok(model);
	}

	/// <inheritdoc cref="ITransactionClient.PutTransferAsync"/>
	/// <response code="201">Successfully created a new transfer.</response>
	/// <response code="204">Successfully replaced an existing transfer.</response>
	/// <response code="404">Transaction with the specified id was not found.</response>
	[HttpPut("{transactionId:guid}/Transfers/{id:guid}")]
	[ProducesResponseType(Status201Created)]
	[ProducesResponseType(Status204NoContent)]
	[ProducesStatus404NotFound]
	public async Task<ActionResult> PutTransfer(Guid transactionId, Guid id, [FromBody] TransferCreation transfer)
	{
		transfer = transfer with { OwnerId = transfer.OwnerId ?? ApplicationUser.Id };

		var transaction = await Repository.FindWriteableByIdAsync(transactionId, ApplicationUser.Id);
		if (transaction is null)
		{
			return await Repository.FindByIdAsync(transactionId) is null
				? NotFound()
				: Forbid();
		}

		var existingTransfer = await _transferRepository.FindWriteableByIdAsync(transactionId, id, ApplicationUser.Id);
		var entity = Mapper.Map<TransferEntity>(transfer) with
		{
			Id = id,
			CreatedByUserId = ApplicationUser.Id,
			ModifiedByUserId = ApplicationUser.Id,
			TransactionId = transactionId,
		};

		if (existingTransfer is null)
		{
			await _transferRepository.AddAsync(entity);
			return CreatedAtAction(nameof(GetTransfer), new { transactionId, id }, null);
		}

		await _transferRepository.UpdateAsync(entity);
		return NoContent();
	}

	/// <inheritdoc cref="ITransactionClient.DeleteTransferAsync"/>
	/// <response code="204">Successfully deleted transfer.</response>
	/// <response code="404">Transfer with the specified id does not exist.</response>
	[HttpDelete("{transactionId:guid}/Transfers/{id:guid}")]
	[ProducesResponseType(Status204NoContent)]
	[ProducesStatus404NotFound]
	public async Task<ActionResult> DeleteTransfer(Guid transactionId, Guid id)
	{
		var transaction = await Repository.FindDeletableByIdAsync(transactionId, ApplicationUser.Id);
		if (transaction is null)
		{
			return NotFound();
		}

		// todo add transaction id
		var deletedCount = await _transferRepository.DeleteAsync(id, ApplicationUser.Id);
		return DeletedEntity<TransferEntity>(id, deletedCount);
	}

	/// <inheritdoc cref="ITransactionClient.GetPurchasesAsync"/>
	/// <response code="200">Successfully got all purchases.</response>
	[HttpGet("{transactionId:guid}/Purchases")]
	[ProducesResponseType(typeof(List<Purchase>), Status200OK)]
	public async Task<List<Purchase>> GetPurchases(
		Guid transactionId,
		CancellationToken cancellationToken)
	{
		var purchases = await _purchaseRepository.GetAllAsync(transactionId, ApplicationUser.Id, cancellationToken);
		var models = purchases.Select(purchase => Mapper.Map<Purchase>(purchase)).ToList();
		return models;
	}

	/// <inheritdoc cref="ITransactionClient.GetPurchaseAsync"/>
	/// <response code="200">Successfully got the purchase with the specified id.</response>
	/// <response code="404">Purchase with the specified id does not exist.</response>
	[HttpGet("{transactionId:guid}/Purchases/{id:guid}")]
	[ProducesResponseType(typeof(Purchase), Status200OK)]
	[ProducesStatus404NotFound]
	public async Task<ActionResult<Purchase>> GetPurchase(
		Guid transactionId,
		Guid id,
		CancellationToken cancellationToken)
	{
		var purchase = await _purchaseRepository.FindByIdAsync(transactionId, id, ApplicationUser.Id, cancellationToken);
		if (purchase is null)
		{
			return NotFound();
		}

		var model = Mapper.Map<Purchase>(purchase);
		return Ok(model);
	}

	/// <inheritdoc cref="ITransactionClient.PutPurchaseAsync"/>
	/// <response code="201">Successfully created a new purchase.</response>
	/// <response code="204">Successfully replaced an existing purchase.</response>
	/// <response code="404">Transaction with the specified id was not found.</response>
	[HttpPut("{transactionId:guid}/Purchases/{id:guid}")]
	[ProducesResponseType(Status201Created)]
	[ProducesResponseType(Status204NoContent)]
	[ProducesStatus404NotFound]
	public async Task<ActionResult> PutPurchase(Guid transactionId, Guid id, [FromBody] PurchaseCreation purchase)
	{
		purchase = purchase with { OwnerId = purchase.OwnerId ?? ApplicationUser.Id };

		var transaction = await Repository.FindWriteableByIdAsync(transactionId, ApplicationUser.Id);
		if (transaction is null)
		{
			return await Repository.FindByIdAsync(transactionId) is null
				? NotFound()
				: Forbid();
		}

		var existingPurchase = await _purchaseRepository.FindWriteableByIdAsync(transactionId, id, ApplicationUser.Id);
		var entity = Mapper.Map<PurchaseEntity>(purchase) with
		{
			Id = id,
			CreatedByUserId = ApplicationUser.Id,
			ModifiedByUserId = ApplicationUser.Id,
			TransactionId = transactionId,
		};

		if (existingPurchase is null)
		{
			await _purchaseRepository.AddAsync(entity);
			return CreatedAtAction(nameof(GetPurchase), new { transactionId, id }, null);
		}

		await _purchaseRepository.UpdateAsync(entity);
		return NoContent();
	}

	/// <inheritdoc cref="ITransactionClient.DeletePurchaseAsync"/>
	/// <response code="204">Successfully deleted purchase.</response>
	/// <response code="404">Purchase with the specified id does not exist.</response>
	[HttpDelete("{transactionId:guid}/Purchases/{id:guid}")]
	[ProducesResponseType(Status204NoContent)]
	[ProducesStatus404NotFound]
	public async Task<ActionResult> DeletePurchase(Guid transactionId, Guid id)
	{
		var transaction = await Repository.FindDeletableByIdAsync(transactionId, ApplicationUser.Id);
		if (transaction is null)
		{
			return NotFound();
		}

		// todo add transaction id
		var deletedCount = await _purchaseRepository.DeleteAsync(id, ApplicationUser.Id);
		return DeletedEntity<PurchaseEntity>(id, deletedCount);
	}

	/// <inheritdoc cref="ITransactionClient.GetLoansAsync"/>
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

	/// <inheritdoc cref="ITransactionClient.GetLoanAsync"/>
	/// <response code="200">Successfully got the loan with the specified id.</response>
	/// <response code="404">Loan with the specified id does not exist.</response>
	[HttpGet("{transactionId:guid}/Loans/{id:guid}")]
	[ProducesResponseType(typeof(Loan), Status200OK)]
	[ProducesStatus404NotFound]
	public async Task<ActionResult> GetLoan(Guid transactionId, Guid id, CancellationToken cancellationToken)
	{
		var loan = await _loanRepository.FindByIdAsync(transactionId, id, ApplicationUser.Id, cancellationToken);
		return loan is null
			? NotFound()
			: Ok(Mapper.Map<Loan>(loan));
	}

	/// <inheritdoc cref="ITransactionClient.PutLoanAsync"/>
	/// <response code="201">Successfully created a new loan.</response>
	/// <response code="204">Successfully replaced an existing loan.</response>
	/// <response code="404">Transaction with the specified id was not found.</response>
	[HttpPut("{transactionId:guid}/Loans/{id:guid}")]
	[ProducesResponseType(Status201Created)]
	[ProducesResponseType(Status204NoContent)]
	[ProducesStatus404NotFound]
	public async Task<ActionResult> PutLoan(Guid transactionId, Guid id, [FromBody] LoanCreation loan)
	{
		loan = loan with { OwnerId = loan.OwnerId ?? ApplicationUser.Id };

		var transaction = await Repository.FindWriteableByIdAsync(transactionId, ApplicationUser.Id);
		if (transaction is null)
		{
			return await Repository.FindByIdAsync(transactionId) is null
				? NotFound()
				: Forbid();
		}

		var existingLoan = await _loanRepository.FindWriteableByIdAsync(transactionId, id, ApplicationUser.Id, CancellationToken.None);
		var entity = Mapper.Map<LoanEntity>(loan) with
		{
			Id = id,
			CreatedByUserId = ApplicationUser.Id,
			ModifiedByUserId = ApplicationUser.Id,
			TransactionId = transactionId,
		};

		if (existingLoan is null)
		{
			await _loanRepository.AddAsync(entity);
			return CreatedAtAction(nameof(GetLoan), new { transactionId, id }, null);
		}

		await _loanRepository.UpdateAsync(entity);
		return NoContent();
	}

	/// <inheritdoc cref="ITransactionClient.DeleteLoanAsync"/>
	/// <response code="204">Successfully deleted loan.</response>
	/// <response code="404">Loan with the specified id does not exist.</response>
	[HttpDelete("{transactionId:guid}/Loans/{id:guid}")]
	[ProducesResponseType(Status204NoContent)]
	[ProducesStatus404NotFound]
	public async Task<ActionResult> DeleteLoan(Guid transactionId, Guid id)
	{
		var transaction = await Repository.FindDeletableByIdAsync(transactionId, ApplicationUser.Id);
		if (transaction is null)
		{
			return NotFound();
		}

		// todo add transaction id
		var deletedCount = await _loanRepository.DeleteAsync(id, ApplicationUser.Id);
		return DeletedEntity<LoanEntity>(id, deletedCount);
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

		_ = await _unitOfWork.UpdateAsync(transaction, user);
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