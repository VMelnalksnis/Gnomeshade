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
using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Products;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;
using Gnomeshade.Interfaces.WebApi.OpenApi;
using Gnomeshade.Interfaces.WebApi.V1_0.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.Interfaces.WebApi.V1_0.Transactions;

/// <summary>CRUD operations on transaction entity.</summary>
public sealed class TransactionController : FinanceControllerBase<TransactionEntity, Transaction>
{
	private readonly TransactionRepository _repository;
	private readonly TransactionItemRepository _itemRepository;
	private readonly ProductRepository _productRepository;
	private readonly ILogger<TransactionController> _logger;
	private readonly TransactionUnitOfWork _unitOfWork;

	/// <summary>Initializes a new instance of the <see cref="TransactionController"/> class.</summary>
	/// <param name="repository">The repository for performing CRUD operations on <see cref="TransactionEntity"/>.</param>
	/// <param name="itemRepository">The repository for performing CRUD operations on <see cref="TransactionItemEntity"/>.</param>
	/// <param name="productRepository">The repository for performing CRUD operations on <see cref="ProductEntity"/>.</param>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="unitOfWork">Unit of work for managing transactions and all related entities.</param>
	/// <param name="applicationUserContext">Context for getting the current application user.</param>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	public TransactionController(
		TransactionRepository repository,
		TransactionItemRepository itemRepository,
		ProductRepository productRepository,
		ILogger<TransactionController> logger,
		TransactionUnitOfWork unitOfWork,
		ApplicationUserContext applicationUserContext,
		Mapper mapper)
		: base(applicationUserContext, mapper)
	{
		_repository = repository;
		_itemRepository = itemRepository;
		_productRepository = productRepository;
		_logger = logger;
		_unitOfWork = unitOfWork;
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
			ValidatedAt = transaction.Validated ? DateTimeOffset.UtcNow : null,
			ValidatedByUserId = transaction.Validated ? ApplicationUser.Id : null,
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
	[HttpGet("Item/{id:guid}")]
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
		var productEntity =
			await _productRepository.GetByIdAsync(itemEntity.ProductId, ApplicationUser.Id, cancellation);
		var product = Mapper.Map<Product>(productEntity);
		var item = Mapper.Map<TransactionItem>(itemEntity) with { Product = product };

		return Ok(item);
	}

	/// <inheritdoc cref="ITransactionClient.PutTransactionItemAsync"/>
	/// <response code="201">A new transaction item was created.</response>
	/// <response code="204">An existing transaction item was replaced.</response>
	/// <response code="404">Transaction with the specified id was not found.</response>
	[HttpPut("{transactionId:guid}/Item/{id:guid}")]
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
	[HttpDelete("Item/{id:guid}")]
	[ProducesResponseType(Status204NoContent)]
	[ProducesStatus404NotFound]
	public async Task<StatusCodeResult> DeleteItem(Guid id)
	{
		var deletedCount = await _itemRepository.DeleteAsync(id, ApplicationUser.Id);
		return deletedCount switch
		{
			0 => NotFound(),
			1 => NoContent(),
			_ => HandleFailedDelete(deletedCount, id),
		};

		StatusCodeResult HandleFailedDelete(int count, Guid transactionItemId)
		{
			_logger.LogError(
				"Deleted {DeletedCount} transaction items by id {TransactionItemId}",
				count,
				transactionItemId);
			return StatusCode(Status500InternalServerError);
		}
	}

	/// <inheritdoc cref="ITransactionClient.TagTransactionItemAsync"/>
	[HttpPut("Item/{id:guid}/Tag/{tagId:guid}")]
	public async Task<StatusCodeResult> TagItem(Guid id, Guid tagId)
	{
		await _itemRepository.TagAsync(id, tagId, ApplicationUser.Id);
		return NoContent();
	}

	/// <inheritdoc cref="ITransactionClient.UntagTransactionItemAsync"/>
	[HttpDelete("Item/{id:guid}/Tag/{tagId:guid}")]
	public async Task<StatusCodeResult> UntagItem(Guid id, Guid tagId)
	{
		await _itemRepository.UntagAsync(id, tagId, ApplicationUser.Id);
		return NoContent();
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
			ValidatedAt = creationModel.Validated ? DateTimeOffset.UtcNow : null,
			ValidatedByUserId = creationModel.Validated ? user.Id : null,
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
