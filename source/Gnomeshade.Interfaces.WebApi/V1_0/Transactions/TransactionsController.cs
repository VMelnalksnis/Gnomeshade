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
using Gnomeshade.Interfaces.WebApi.Models.Products;
using Gnomeshade.Interfaces.WebApi.Models.Tags;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;
using Gnomeshade.Interfaces.WebApi.OpenApi;
using Gnomeshade.Interfaces.WebApi.V1_0.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.Interfaces.WebApi.V1_0.Transactions;

/// <summary>CRUD operations on transaction entity.</summary>
public sealed class TransactionsController : FinanceControllerBase<TransactionEntity, Transaction>
{
	private readonly TransactionRepository _repository;
	private readonly TransactionItemRepository _itemRepository;
	private readonly TransferRepository _transferRepository;
	private readonly PurchaseRepository _purchaseRepository;
	private readonly ProductRepository _productRepository;
	private readonly TransactionUnitOfWork _unitOfWork;

	/// <summary>Initializes a new instance of the <see cref="TransactionsController"/> class.</summary>
	/// <param name="repository">The repository for performing CRUD operations on <see cref="TransactionEntity"/>.</param>
	/// <param name="itemRepository">The repository for performing CRUD operations on <see cref="TransactionItemEntity"/>.</param>
	/// <param name="productRepository">The repository for performing CRUD operations on <see cref="ProductEntity"/>.</param>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="unitOfWork">Unit of work for managing transactions and all related entities.</param>
	/// <param name="applicationUserContext">Context for getting the current application user.</param>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	/// <param name="transferRepository">Persistence store for <see cref="TransferEntity"/>.</param>
	/// <param name="purchaseRepository">Persistence store for <see cref="PurchaseEntity"/>.</param>
	public TransactionsController(
		TransactionRepository repository,
		TransactionItemRepository itemRepository,
		ProductRepository productRepository,
		ILogger<TransactionsController> logger,
		TransactionUnitOfWork unitOfWork,
		ApplicationUserContext applicationUserContext,
		Mapper mapper,
		TransferRepository transferRepository,
		PurchaseRepository purchaseRepository)
		: base(applicationUserContext, mapper, logger)
	{
		_repository = repository;
		_itemRepository = itemRepository;
		_productRepository = productRepository;
		_unitOfWork = unitOfWork;
		_transferRepository = transferRepository;
		_purchaseRepository = purchaseRepository;
	}

	/// <inheritdoc cref="ITransactionClient.GetTransactionAsync"/>
	/// <response code="200">Transaction with the specified id exists.</response>
	/// <response code="404">Transaction with the specified id does not exist.</response>
	[HttpGet("{id:guid}")]
	[ProducesResponseType(typeof(Transaction), Status200OK)]
	[ProducesStatus404NotFound]
	public async Task<ActionResult<Transaction>> Get(Guid id, CancellationToken cancellation)
	{
		return await Find(() => _repository.FindByIdAsync(id, ApplicationUser.Id, cancellation));
	}

	/// <summary>Gets all transactions.</summary>
	/// <param name="timeRange">A time range for filtering transactions.</param>
	/// <param name="cancellation">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns><see cref="OkObjectResult"/> with the transactions.</returns>
	/// <response code="200">Successfully got all transactions.</response>
	[HttpGet]
	[ProducesResponseType(typeof(List<Transaction>), Status200OK)]
	public async Task<ActionResult<List<Transaction>>> GetAll(
		[FromQuery] OptionalTimeRange timeRange,
		CancellationToken cancellation)
	{
		var (fromDate, toDate) = TimeRange.FromOptional(timeRange, DateTimeOffset.UtcNow);

		var transactions = await _repository.GetAllAsync(fromDate, toDate, ApplicationUser.Id, cancellation);
		var transactionModels = transactions.Select(MapToModel).ToList();
		return Ok(transactionModels);
	}

	/// <inheritdoc cref="ITransactionClient.CreateTransactionAsync"/>
	/// <response code="201">Transaction was successfully created.</response>
	[HttpPost]
	[ProducesResponseType(typeof(Guid), Status201Created)]
	public async Task<ActionResult<Guid>> Post([FromBody] TransactionCreationModel transaction)
	{
		var entity = Mapper.Map<TransactionEntity>(transaction) with
		{
			OwnerId = ApplicationUser.Id, // todo
			CreatedByUserId = ApplicationUser.Id,
			ModifiedByUserId = ApplicationUser.Id,
			ImportedAt = transaction.ImportHash is null ? null : DateTimeOffset.UtcNow,
			ReconciledByUserId = transaction.ReconciledAt is null ? null : ApplicationUser.Id,
		};

		var transactionId = await _unitOfWork.AddAsync(entity);
		return CreatedAtAction(nameof(Get), new { id = transactionId }, transactionId);
	}

	/// <inheritdoc cref="ITransactionClient.PutTransactionAsync"/>
	[HttpPut("{id:guid}")]
	[ProducesResponseType(Status201Created)]
	[ProducesResponseType(Status204NoContent)]
	public async Task<ActionResult> Put(Guid id, [FromBody] TransactionCreationModel transaction)
	{
		var existingTransaction = await _repository.FindByIdAsync(id, ApplicationUser.Id);

		return existingTransaction is null
			? await CreateNewTransactionAsync(transaction, ApplicationUser, id)
			: await UpdateExistingTransactionAsync(transaction, ApplicationUser, existingTransaction);
	}

	/// <inheritdoc cref="ITransactionClient.DeleteTransactionAsync"/>
	/// <response code="204">Transaction was successfully deleted.</response>
	/// <response code="404">Transaction with the specified id does not exist.</response>
	[HttpDelete("{id:guid}")]
	[ProducesResponseType(Status204NoContent)]
	[ProducesStatus404NotFound]
	public async Task<StatusCodeResult> Delete(Guid id)
	{
		var transaction = await _repository.FindByIdAsync(id, ApplicationUser.Id);
		if (transaction is null)
		{
			return NotFound();
		}

		_ = await _unitOfWork.DeleteAsync(transaction, ApplicationUser.Id);
		return NoContent();
	}

	/// <inheritdoc cref="ITransactionClient.GetTransactionItemAsync"/>
	/// <response code="200">Transaction item with the specified id exists.</response>
	/// <response code="404">Transaction item with the specified id does not exist.</response>
	[HttpGet("Items/{id:guid}")]
	[ProducesResponseType(typeof(TransactionItem), Status200OK)]
	[ProducesStatus404NotFound]
	public async Task<ActionResult<TransactionItem>> GetItem(Guid id, CancellationToken cancellation)
	{
		var itemEntity = await _itemRepository.FindByIdAsync(id, ApplicationUser.Id, cancellation);
		if (itemEntity is null)
		{
			return NotFound();
		}

		// todo this is already done with one query when getting transaction with all items
		var productEntity = await _productRepository.GetByIdAsync(itemEntity.ProductId, ApplicationUser.Id, cancellation);
		var product = Mapper.Map<Product>(productEntity);
		var item = Mapper.Map<TransactionItem>(itemEntity) with { Product = product };

		return Ok(item);
	}

	/// <inheritdoc cref="ITransactionClient.PutTransactionItemAsync"/>
	/// <response code="201">A new transaction item was created.</response>
	/// <response code="204">An existing transaction item was replaced.</response>
	/// <response code="404">Transaction with the specified id was not found.</response>
	[HttpPut("{transactionId:guid}/Items/{id:guid}")]
	[ProducesResponseType(Status201Created)]
	[ProducesResponseType(Status204NoContent)]
	[ProducesStatus404NotFound]
	public async Task<ActionResult> PutItem(Guid id, Guid transactionId, [FromBody] TransactionItemCreationModel item)
	{
		if (await _repository.FindByIdAsync(transactionId, ApplicationUser.Id) is null)
		{
			return NotFound();
		}

		var existingItem = await _itemRepository.FindByIdAsync(id, ApplicationUser.Id);
		return existingItem is null
			? await CreateNewItemAsync(transactionId, item, ApplicationUser, id)
			: await UpdateExistingItemAsync(transactionId, item, ApplicationUser, id);
	}

	/// <inheritdoc cref="ITransactionClient.DeleteTransactionItemAsync"/>
	/// <response code="204">Transaction item was successfully deleted.</response>
	/// <response code="404">Transaction item with the specified id does not exist.</response>
	[HttpDelete("Items/{id:guid}")]
	[ProducesResponseType(Status204NoContent)]
	[ProducesStatus404NotFound]
	public async Task<StatusCodeResult> DeleteItem(Guid id)
	{
		var deletedCount = await _itemRepository.DeleteAsync(id, ApplicationUser.Id);
		return DeletedEntity<TransactionItem>(id, deletedCount);
	}

	/// <inheritdoc cref="ITransactionClient.GetTransactionItemTagsAsync"/>
	/// <response code="200">Successfully got the tags.</response>
	[HttpGet("Items/{id:guid}/Tags")]
	[ProducesResponseType(typeof(List<Tag>), Status200OK)]
	public async Task<ActionResult<List<Tag>>> GetTags(Guid id, CancellationToken cancellationToken = default)
	{
		var tagEntities = await _itemRepository.GetTagsAsync(id, ApplicationUser.Id, cancellationToken);
		var tags = tagEntities.Select(entity => Mapper.Map<Tag>(entity)).ToList();
		return Ok(tags);
	}

	/// <inheritdoc cref="ITransactionClient.TagTransactionItemAsync"/>
	[HttpPut("Items/{id:guid}/Tags/{tagId:guid}")]
	public async Task<StatusCodeResult> TagItem(Guid id, Guid tagId)
	{
		await _itemRepository.TagAsync(id, tagId, ApplicationUser.Id);
		return NoContent();
	}

	/// <inheritdoc cref="ITransactionClient.UntagTransactionItemAsync"/>
	[HttpDelete("Items/{id:guid}/Tags/{tagId:guid}")]
	public async Task<StatusCodeResult> UntagItem(Guid id, Guid tagId)
	{
		await _itemRepository.UntagAsync(id, tagId, ApplicationUser.Id);
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
		var transfers = await _transferRepository.GetAllAsync(ApplicationUser.Id, cancellationToken);
		var models = transfers.Select(transfer => Mapper.Map<Transfer>(transfer)).ToList();
		return models;
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
		if (await _repository.FindByIdAsync(transactionId, ApplicationUser.Id) is null)
		{
			return NotFound();
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
	public async Task<StatusCodeResult> DeleteTransfer(Guid transactionId, Guid id)
	{
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
		var purchases = await _purchaseRepository.GetAllAsync(ApplicationUser.Id, cancellationToken);
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
		if (await _repository.FindByIdAsync(transactionId, ApplicationUser.Id) is null)
		{
			return NotFound();
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
	public async Task<StatusCodeResult> DeletePurchase(Guid transactionId, Guid id)
	{
		// todo add transaction id
		var deletedCount = await _purchaseRepository.DeleteAsync(id, ApplicationUser.Id);
		return DeletedEntity<PurchaseEntity>(id, deletedCount);
	}

	private async Task<CreatedAtActionResult> CreateNewTransactionAsync(
		TransactionCreationModel creationModel,
		UserEntity user,
		Guid id)
	{
		var transaction = Mapper.Map<TransactionEntity>(creationModel) with
		{
			Id = id,
			OwnerId = user.Id, // todo
			CreatedByUserId = user.Id,
			ModifiedByUserId = user.Id,
			ImportedAt = creationModel.ImportHash is null ? null : DateTimeOffset.UtcNow,
			ReconciledByUserId = creationModel.ReconciledAt is null ? null : user.Id,
		};

		var transactionId = await _unitOfWork.AddAsync(transaction);
		return CreatedAtAction(nameof(Get), new { id = transactionId }, string.Empty);
	}

	private async Task<NoContentResult> UpdateExistingTransactionAsync(
		TransactionCreationModel creationModel,
		UserEntity user,
		TransactionEntity existingTransaction)
	{
		var transaction = Mapper.Map<TransactionEntity>(creationModel) with
		{
			Id = existingTransaction.Id,
			OwnerId = user.Id, // todo only works for entities created by the user
			ModifiedByUserId = user.Id,
		};

		_ = await _unitOfWork.UpdateAsync(transaction, user);
		return NoContent();
	}

	private async Task<CreatedAtActionResult> CreateNewItemAsync(
		Guid transactionId,
		TransactionItemCreationModel creationModel,
		UserEntity user,
		Guid id)
	{
		var transactionItem = Mapper.Map<TransactionItemEntity>(creationModel) with
		{
			Id = id,
			OwnerId = user.Id, // todo
			CreatedByUserId = user.Id,
			ModifiedByUserId = user.Id,
			TransactionId = transactionId,
		};

		await _itemRepository.AddAsync(transactionItem);
		return CreatedAtAction(nameof(GetItem), new { id }, string.Empty);
	}

	private async Task<NoContentResult> UpdateExistingItemAsync(
		Guid transactionId,
		TransactionItemCreationModel creationModel,
		UserEntity user,
		Guid id)
	{
		var transactionItem = Mapper.Map<TransactionItemEntity>(creationModel) with
		{
			Id = id,
			OwnerId = user.Id, // todo only works for entities created by the user
			ModifiedByUserId = user.Id,
			TransactionId = transactionId,
		};

		await _itemRepository.UpdateAsync(transactionItem);
		return NoContent();
	}
}
