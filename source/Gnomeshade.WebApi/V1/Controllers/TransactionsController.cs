﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
using Gnomeshade.WebApi.V1.Transactions;

using Microsoft.AspNetCore.Mvc;

using NodaTime;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.WebApi.V1.Controllers;

/// <summary>CRUD operations on transaction entity.</summary>
public sealed class TransactionsController : CreatableBase<TransactionRepository, TransactionEntity, Transaction, TransactionCreation>
{
	private readonly TransferRepository _transferRepository;
	private readonly PurchaseRepository _purchaseRepository;
#pragma warning disable CS0612 // Type or member is obsolete
	private readonly LoanRepository _loanRepository;
#pragma warning restore CS0612 // Type or member is obsolete
	private readonly CounterpartyRepository _counterpartyRepository;
	private readonly AccountRepository _accountRepository;

	/// <summary>Initializes a new instance of the <see cref="TransactionsController"/> class.</summary>
	/// <param name="repository">The repository for performing CRUD operations on <see cref="TransactionEntity"/>.</param>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	/// <param name="transferRepository">Persistence store for <see cref="TransferEntity"/>.</param>
	/// <param name="purchaseRepository">Persistence store for <see cref="PurchaseEntity"/>.</param>
	/// <param name="loanRepository">Persistence store for <see cref="LoanEntity"/>.</param>
	/// <param name="counterpartyRepository">Persistence store for <see cref="CounterpartyEntity"/>.</param>
	/// <param name="accountRepository">Persistence store for <see cref="AccountEntity"/>.</param>
	/// <param name="dbConnection">Database connection for transaction management.</param>
	public TransactionsController(
		TransactionRepository repository,
		Mapper mapper,
		TransferRepository transferRepository,
		PurchaseRepository purchaseRepository,
#pragma warning disable CS0612 // Type or member is obsolete
		LoanRepository loanRepository,
#pragma warning restore CS0612 // Type or member is obsolete
		CounterpartyRepository counterpartyRepository,
		AccountRepository accountRepository,
		DbConnection dbConnection)
		: base(mapper, repository, dbConnection)
	{
		_transferRepository = transferRepository;
		_purchaseRepository = purchaseRepository;
		_loanRepository = loanRepository;
		_counterpartyRepository = counterpartyRepository;
		_accountRepository = accountRepository;
	}

	/// <summary>Gets all transactions.</summary>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns><see cref="OkObjectResult"/> with the transactions.</returns>
	/// <response code="200">Successfully got all transactions.</response>
	[ProducesResponseType<List<Transaction>>(Status200OK)]
	public override Task<List<Transaction>> Get(CancellationToken cancellationToken) =>
		base.Get(cancellationToken);

	/// <inheritdoc cref="ITransactionClient.GetTransactionAsync"/>
	/// <response code="200">Transaction with the specified id exists.</response>
	/// <response code="404">Transaction with the specified id does not exist.</response>
	[ProducesResponseType<Transaction>(Status200OK)]
	public override Task<ActionResult<Transaction>> Get(Guid id, CancellationToken cancellationToken) =>
		base.Get(id, cancellationToken);

	/// <inheritdoc cref="ITransactionClient.GetDetailedTransactionAsync"/>
	/// <response code="200">Transaction with the specified id exists.</response>
	/// <response code="404">Transaction with the specified id does not exist.</response>
	[Obsolete]
	[HttpGet("{id:guid}/Details")]
#pragma warning disable CS0612 // Type or member is obsolete
	[ProducesResponseType<Transactions.DetailedTransaction>(Status200OK)]
#pragma warning restore CS0612 // Type or member is obsolete
	[ProducesStatus404NotFound]
	public async Task<ActionResult<Transactions.DetailedTransaction>> GetDetailed(Guid id, CancellationToken cancellationToken)
	{
		var transaction = await Repository.FindDetailedAsync(id, ApplicationUser.Id, cancellationToken);
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
	[Obsolete]
	[HttpGet("Details")]
#pragma warning disable CS0612 // Type or member is obsolete
	[ProducesResponseType<List<Transactions.DetailedTransaction>>(Status200OK)]
#pragma warning restore CS0612 // Type or member is obsolete
	public async IAsyncEnumerable<Transactions.DetailedTransaction> GetAllDetailed(
		[FromQuery] OptionalTimeRange timeRange,
		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var (fromDate, toDate) = TimeRange.FromOptional(timeRange, SystemClock.Instance.GetCurrentInstant());
		var transactions =
			await Repository.GetAllDetailedAsync(fromDate, toDate, ApplicationUser.Id, cancellationToken);
		var userAccountsInCurrencyIds = await GetUserAccountsInCurrencyIds(cancellationToken);

		foreach (var transaction in transactions)
		{
			yield return ToDetailed(transaction, userAccountsInCurrencyIds);
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

	// ReSharper disable once RedundantOverriddenMember

	/// <inheritdoc cref="ITransactionClient.DeleteTransactionAsync"/>
	/// <response code="204">Transaction was successfully deleted.</response>
	/// <response code="404">Transaction with the specified id does not exist.</response>
	/// <response code="409">Transaction cannot be deleted because some other entity is still referencing it.</response>
	public override Task<ActionResult> Delete(Guid id) =>
		base.Delete(id);

	/// <inheritdoc cref="ITransactionClient.GetTransactionLinksAsync"/>
	/// <response code="200">Successfully got all links.</response>
	[HttpGet("{transactionId:guid}/Links")]
	[ProducesResponseType<List<Link>>(Status200OK)]
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
		var userId = ApplicationUser.Id;
		await using var dbTransaction = await DbConnection.OpenAndBeginTransaction();

		var transaction = await Repository.FindByIdAsync(transactionId, userId, dbTransaction);
		if (transaction is null)
		{
			return NotFound();
		}

		await Repository.AddLinkAsync(transactionId, id, userId, dbTransaction);
		await dbTransaction.CommitAsync();
		return NoContent();
	}

	/// <inheritdoc cref="ITransactionClient.RemoveLinkFromTransactionAsync"/>
	/// <response code="204">Successfully removed link to transaction.</response>
	[HttpDelete("{transactionId:guid}/Links/{id:guid}")]
	[ProducesResponseType(Status204NoContent)]
	[ProducesStatus404NotFound]
	public async Task<ActionResult> RemoveLink(Guid transactionId, Guid id)
	{
		var userId = ApplicationUser.Id;
		await using var dbTransaction = await DbConnection.OpenAndBeginTransaction();

		var transaction = await Repository.FindByIdAsync(transactionId, userId);
		if (transaction is null)
		{
			return NotFound();
		}

		await Repository.RemoveLinkAsync(transactionId, id, userId, dbTransaction);
		await dbTransaction.CommitAsync();
		return NoContent();
	}

	/// <inheritdoc cref="ITransactionClient.GetTransfersAsync(Guid, CancellationToken)"/>
	/// <response code="200">Successfully got all transfers.</response>
	[HttpGet("{transactionId:guid}/Transfers")]
	[ProducesResponseType<List<Transfer>>(Status200OK)]
	public async Task<List<Transfer>> GetTransfers(Guid transactionId, CancellationToken cancellationToken)
	{
		var transfers = await _transferRepository.GetAllAsync(transactionId, ApplicationUser.Id, cancellationToken);
		return transfers.Select(transfer => Mapper.Map<Transfer>(transfer)).ToList();
	}

	/// <inheritdoc cref="ITransactionClient.GetPurchasesAsync(Guid, CancellationToken)"/>
	/// <response code="200">Successfully got all purchases.</response>
	[HttpGet("{transactionId:guid}/Purchases")]
	[ProducesResponseType<List<Purchase>>(Status200OK)]
	public async Task<List<Purchase>> GetPurchases(Guid transactionId, CancellationToken cancellationToken)
	{
		var purchases = await _purchaseRepository.GetAllAsync(transactionId, ApplicationUser.Id, cancellationToken);
		var models = purchases.Select(purchase => Mapper.Map<Purchase>(purchase)).ToList();
		return models;
	}

	/// <summary>Gets all loans for the specified transaction.</summary>
	/// <param name="transactionId">The id of the transaction for which to get all the loans.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>All loans for the specified transaction.</returns>
	[Obsolete]
	[HttpGet("{transactionId:guid}/Loans")]
#pragma warning disable CS0612 // Type or member is obsolete
	[ProducesResponseType<List<Loan>>(Status200OK)]
#pragma warning restore CS0612 // Type or member is obsolete
	public async Task<List<Loan>> GetLoans(Guid transactionId, CancellationToken cancellationToken)
	{
		var loans = await _loanRepository.GetAllAsync(transactionId, ApplicationUser.Id, cancellationToken);
		return loans.Select(loan => Mapper.Map<Loan>(loan)).ToList();
	}

	/// <summary>Gets all loans issued or received by the specified counterparty.</summary>
	/// <param name="counterpartyId">The id of the counterparty for which to get all the loans for.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>All loans for the specified counterparty.</returns>
	/// <response code="200">Successfully got all loans.</response>
	[Obsolete]
	[HttpGet("Loans")]
#pragma warning disable CS0612 // Type or member is obsolete
	[ProducesResponseType<List<Loan>>(Status200OK)]
#pragma warning restore CS0612 // Type or member is obsolete
	public async Task<List<Loan>> GetCounterpartyLoans(Guid counterpartyId, CancellationToken cancellationToken)
	{
		var loans = await _loanRepository.GetAllForCounterpartyAsync(counterpartyId, ApplicationUser.Id, cancellationToken);
		return loans.Select(loan => Mapper.Map<Loan>(loan)).ToList();
	}

	/// <inheritdoc cref="ITransactionClient.MergeTransactionsAsync(Guid, IEnumerable{Guid})"/>
	/// <response code="204">Transactions were successfully merged.</response>
	[HttpPost("{targetId:guid}/Merge")]
	[ProducesResponseType(Status204NoContent)]
	public async Task<ActionResult> Merge(Guid targetId, [FromQuery, MinLength(1)] Guid[] sourceIds)
	{
		if (sourceIds.Contains(targetId))
		{
			ModelState.AddModelError(nameof(sourceIds), "Cannot merge transaction into itself");
			return BadRequest();
		}

		await using var dbTransaction = await DbConnection.OpenAndBeginTransaction();

		var userId = ApplicationUser.Id;
		foreach (var sourceId in sourceIds)
		{
			await Repository.MergeAsync(targetId, sourceId, userId, dbTransaction);
		}

		await dbTransaction.CommitAsync();
		return NoContent();
	}

	/// <inheritdoc cref="ITransactionClient.GetRelatedTransactionAsync"/>
	/// <response code="200">Successfully got all related transactions.</response>
	[HttpGet("{id:guid}/Related")]
	[ProducesResponseType<List<Transaction>>(Status200OK)]
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
		UserEntity user,
		DbTransaction dbTransaction)
	{
		var transaction = Mapper.Map<TransactionEntity>(creation) with
		{
			Id = id,
			ModifiedByUserId = user.Id,
		};

		var updatedCount = await Repository.UpdateAsync(transaction, dbTransaction);
		return updatedCount is 1
			? NoContent()
			: throw new InvalidOperationException("Failed to update transaction");
	}

	/// <inheritdoc />
	protected override async Task<ActionResult> CreateNewAsync(
		Guid id,
		TransactionCreation creation,
		UserEntity user,
		DbTransaction dbTransaction)
	{
		var transaction = Mapper.Map<TransactionEntity>(creation) with
		{
			Id = id,
			CreatedByUserId = user.Id,
			ModifiedByUserId = user.Id,
			ImportedAt = creation.ImportedAt ??
			(creation.ImportHash is null ? null : SystemClock.Instance.GetCurrentInstant()),
			ReconciledByUserId = creation.ReconciledAt is null ? null : user.Id,
		};

		var transactionId = await Repository.AddAsync(transaction, dbTransaction);
		return CreatedAtAction(nameof(Get), new { id = transactionId }, id);
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

	[Obsolete]
	private Transactions.DetailedTransaction ToDetailed(DetailedTransactionEntity transaction, List<Guid> userAccountsInCurrencyIds)
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
		var loanTotal = transaction.Loans.Sum(loan => loan.Amount);

		return Mapper.Map<Transactions.DetailedTransaction>(transaction) with
		{
			Transfers = transaction.Transfers.Select(transfer => Mapper.Map<Transfer>(transfer)).ToList(),
			TransferBalance = transferBalance,
			Purchases = transaction.Purchases.Select(purchase => Mapper.Map<Purchase>(purchase)).ToList(),
			PurchaseTotal = purchaseTotal,
			Loans = transaction.Loans.Select(loan => Mapper.Map<Loan>(loan)).ToList(),
			LoanTotal = loanTotal,
			Links = transaction.Links.Select(link => Mapper.Map<Link>(link)).ToList(),
		};
	}
}
