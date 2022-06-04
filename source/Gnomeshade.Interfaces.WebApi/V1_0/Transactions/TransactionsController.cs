// Copyright 2021 Valters Melnalksnis
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
using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;
using Gnomeshade.Interfaces.WebApi.OpenApi;
using Gnomeshade.Interfaces.WebApi.V1_0.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using NodaTime;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.Interfaces.WebApi.V1_0.Transactions;

/// <summary>CRUD operations on transaction entity.</summary>
public sealed class TransactionsController : CreatableBase<TransactionRepository, TransactionEntity, Transaction, TransactionCreation>
{
	private readonly TransferRepository _transferRepository;
	private readonly PurchaseRepository _purchaseRepository;
	private readonly LoanRepository _loanRepository;
	private readonly TransactionUnitOfWork _unitOfWork;

	/// <summary>Initializes a new instance of the <see cref="TransactionsController"/> class.</summary>
	/// <param name="repository">The repository for performing CRUD operations on <see cref="TransactionEntity"/>.</param>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="unitOfWork">Unit of work for managing transactions and all related entities.</param>
	/// <param name="applicationUserContext">Context for getting the current application user.</param>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	/// <param name="transferRepository">Persistence store for <see cref="TransferEntity"/>.</param>
	/// <param name="purchaseRepository">Persistence store for <see cref="PurchaseEntity"/>.</param>
	/// <param name="loanRepository">Persistence store for <see cref="LoanEntity"/>.</param>
	public TransactionsController(
		TransactionRepository repository,
		ILogger<TransactionsController> logger,
		TransactionUnitOfWork unitOfWork,
		ApplicationUserContext applicationUserContext,
		Mapper mapper,
		TransferRepository transferRepository,
		PurchaseRepository purchaseRepository,
		LoanRepository loanRepository)
		: base(applicationUserContext, mapper, logger, repository)
	{
		_unitOfWork = unitOfWork;
		_transferRepository = transferRepository;
		_purchaseRepository = purchaseRepository;
		_loanRepository = loanRepository;
	}

	/// <inheritdoc cref="ITransactionClient.GetTransactionAsync"/>
	/// <response code="200">Transaction with the specified id exists.</response>
	/// <response code="404">Transaction with the specified id does not exist.</response>
	[ProducesResponseType(typeof(Transaction), Status200OK)]
	public override Task<ActionResult<Transaction>> Get(Guid id, CancellationToken cancellationToken)
	{
		return base.Get(id, cancellationToken);
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
	[HttpDelete("{id:guid}")]
	[ProducesResponseType(Status204NoContent)]
	[ProducesStatus404NotFound]
	public override async Task<StatusCodeResult> Delete(Guid id)
	{
		var transaction = await Repository.FindByIdAsync(id, ApplicationUser.Id);
		if (transaction is null)
		{
			return NotFound();
		}

		_ = await _unitOfWork.DeleteAsync(transaction, ApplicationUser.Id);
		return NoContent();
	}

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
		var transaction = await Repository.FindByIdAsync(transactionId, ApplicationUser.Id);
		if (transaction is null)
		{
			return await Repository.FindByIdAsync(transactionId) is null
				? NotFound()
				: Forbid();
		}

		var existingTransfer = await _transferRepository.FindByIdAsync(transactionId, id, ApplicationUser.Id);
		var entity = Mapper.Map<TransferEntity>(transfer) with
		{
			Id = id,
			CreatedByUserId = ApplicationUser.Id,
			OwnerId = ApplicationUser.Id, // todo
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
		var transaction = await Repository.FindByIdAsync(transactionId, ApplicationUser.Id);
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
		var transaction = await Repository.FindByIdAsync(transactionId, ApplicationUser.Id);
		if (transaction is null)
		{
			return await Repository.FindByIdAsync(transactionId) is null
				? NotFound()
				: Forbid();
		}

		var existingPurchase = await _purchaseRepository.FindByIdAsync(transactionId, id, ApplicationUser.Id);
		var entity = Mapper.Map<PurchaseEntity>(purchase) with
		{
			Id = id,
			CreatedByUserId = ApplicationUser.Id,
			OwnerId = ApplicationUser.Id, // todo
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
		var transaction = await Repository.FindByIdAsync(transactionId, ApplicationUser.Id);
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
		var transaction = await Repository.FindByIdAsync(transactionId, ApplicationUser.Id);
		if (transaction is null)
		{
			return await Repository.FindByIdAsync(transactionId) is null
				? NotFound()
				: Forbid();
		}

		var existingLoan = await _loanRepository.FindByIdAsync(transactionId, id, ApplicationUser.Id, CancellationToken.None);
		var entity = Mapper.Map<LoanEntity>(loan) with
		{
			Id = id,
			CreatedByUserId = ApplicationUser.Id,
			OwnerId = ApplicationUser.Id, // todo
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
		var transaction = await Repository.FindByIdAsync(transactionId, ApplicationUser.Id);
		if (transaction is null)
		{
			return NotFound();
		}

		// todo add transaction id
		var deletedCount = await _loanRepository.DeleteAsync(id, ApplicationUser.Id);
		return DeletedEntity<LoanEntity>(id, deletedCount);
	}

	/// <inheritdoc />
	protected override async Task<ActionResult> UpdateExistingAsync(Guid id, TransactionCreation creation, UserEntity user)
	{
		var transaction = Mapper.Map<TransactionEntity>(creation) with
		{
			Id = id,
			OwnerId = user.Id, // todo only works for entities created by the user
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
			OwnerId = user.Id, // todo
			CreatedByUserId = user.Id,
			ModifiedByUserId = user.Id,
			ImportedAt = creation.ImportHash is null ? null : SystemClock.Instance.GetCurrentInstant(),
			ReconciledByUserId = creation.ReconciledAt is null ? null : user.Id,
		};

		var transactionId = await _unitOfWork.AddAsync(transaction);
		return CreatedAtAction(nameof(Get), new { id = transactionId }, id);
	}
}
