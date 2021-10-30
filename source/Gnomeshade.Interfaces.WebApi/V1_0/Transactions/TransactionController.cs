// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using Gnomeshade.Data;
using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;
using Gnomeshade.Interfaces.WebApi.Models.Products;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;
using Gnomeshade.Interfaces.WebApi.V1_0.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.Interfaces.WebApi.V1_0.Transactions
{
	/// <summary>
	/// CRUD operations on transaction entity.
	/// </summary>
	[SuppressMessage(
		"ReSharper",
		"AsyncConverter.ConfigureAwaitHighlighting",
		Justification = "ASP.NET Core doesn't have a SynchronizationContext")]
	public sealed class TransactionController : FinanceControllerBase<TransactionEntity, Transaction>
	{
		private readonly TransactionRepository _repository;
		private readonly TransactionItemRepository _itemRepository;
		private readonly ProductRepository _productRepository;
		private readonly ILogger<TransactionController> _logger;
		private readonly TransactionUnitOfWork _unitOfWork;

		/// <summary>
		/// Initializes a new instance of the <see cref="TransactionController"/> class.
		/// </summary>
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

		/// <summary>
		/// Gets a transaction by the specified id.
		/// </summary>
		/// <param name="id">The id of the transaction to get.</param>
		/// <param name="cancellation">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
		/// <returns><see cref="OkObjectResult"/> if transaction was found, otherwise <see cref="NotFoundResult"/>.</returns>
		/// <response code="200">Transaction with the specified id exists.</response>
		/// <response code="404">Transaction with the specified id does not exist.</response>
		[HttpGet("{id:guid}")]
		[ProducesResponseType(Status200OK)]
		[ProducesResponseType(typeof(ProblemDetails), Status404NotFound)]
		public async Task<ActionResult<Transaction>> Get(Guid id, CancellationToken cancellation)
		{
			return await Find(() => _repository.FindByIdAsync(id, cancellation));
		}

		/// <summary>
		/// Gets all transactions.
		/// </summary>
		/// <param name="timeRange">A time range for filtering transactions.</param>
		/// <param name="cancellation">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
		/// <returns><see cref="OkObjectResult"/> with the transactions.</returns>
		/// <response code="200">Successfully got all transactions.</response>
		[HttpGet]
		[ProducesResponseType(Status200OK)]
		public async Task<ActionResult<IEnumerable<Transaction>>> GetAll(
			[FromQuery] OptionalTimeRange timeRange,
			CancellationToken cancellation)
		{
			var (fromDate, toDate) = TimeRange.FromOptional(timeRange, DateTimeOffset.UtcNow);

			var transactions = await _repository.GetAllAsync(fromDate, toDate, cancellation);
			var transactionModels = transactions.Select(MapToModel).ToList();
			return Ok(transactionModels);
		}

		/// <summary>
		/// Creates a new transaction.
		/// </summary>
		/// <param name="model">The transaction that will be created.</param>
		/// <returns><see cref="CreatedAtActionResult"/> with the id of the transaction.</returns>
		/// <response code="201">Transaction was successfully created.</response>
		[HttpPost]
		[ProducesResponseType(Status201Created)]
		public async Task<ActionResult<Guid>> Create([FromBody, BindRequired] TransactionCreationModel model)
		{
			return await CreateNewTransactionAsync(model, ApplicationUser);
		}

		/// <summary>
		/// Creates a new transaction with items, or updates items for an existing transaction.
		/// </summary>
		/// <param name="model">The transaction to create or replace.</param>
		/// <returns>The id of the created or replaced transaction.</returns>
		[HttpPut]
		[ProducesResponseType(Status200OK)]
		[ProducesResponseType(Status201Created)]
		[ProducesResponseType(Status404NotFound)]
		public async Task<ActionResult<Guid>> Put([FromBody, BindRequired] TransactionCreationModel model)
		{
			var existingTransaction = model.Id is not null
				? await _repository.FindByIdAsync(model.Id.Value)
				: null;

			if (model.Id is not null && existingTransaction is null)
			{
				return NotFound();
			}

			return existingTransaction is null
				? await CreateNewTransactionAsync(model, ApplicationUser)
				: await UpdateExistingTransactionAsync(model, ApplicationUser, existingTransaction);
		}

		/// <summary>
		/// Deletes the specified transaction.
		/// </summary>
		/// <param name="id">The id of the transaction to delete.</param>
		/// <returns><see cref="NoContentResult"/> if transaction was deleted successfully, otherwise <see cref="NotFoundResult"/>.</returns>
		/// <response code="204">Transaction was successfully deleted.</response>
		/// <response code="404">Transaction with the specified id does not exist.</response>
		[HttpDelete("{id:guid}")]
		[ProducesResponseType(Status204NoContent)]
		[ProducesResponseType(typeof(ProblemDetails), Status404NotFound)]
		public async Task<StatusCodeResult> Delete(Guid id)
		{
			var transaction = await _repository.FindByIdAsync(id);
			if (transaction is null)
			{
				return NotFound();
			}

			_ = await _unitOfWork.DeleteAsync(transaction);
			return NoContent();
		}

		/// <summary>
		/// Gets a transaction item by the specified id.
		/// </summary>
		/// <param name="id">The id of the transaction item to get.</param>
		/// <param name="cancellation">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
		/// <returns><see cref="OkObjectResult"/> if transaction item was found, otherwise <see cref="NotFoundResult"/>.</returns>
		/// <response code="200">Transaction item with the specified id exists.</response>
		/// <response code="404">Transaction item with the specified id does not exist.</response>
		[HttpGet("Item/{id:guid}")]
		public async Task<ActionResult<TransactionItem>> GetItem(Guid id, CancellationToken cancellation)
		{
			var itemEntity = await _itemRepository.FindByIdAsync(id, cancellation);
			if (itemEntity is null)
			{
				return NotFound();
			}

			// todo this is already done with one query when getting transaction with all items
			var productEntity = await _productRepository.GetByIdAsync(itemEntity.ProductId, cancellation);
			var product = Mapper.Map<Product>(productEntity);
			var item = Mapper.Map<TransactionItem>(itemEntity) with { Product = product };

			return Ok(item);
		}

		/// <summary>
		/// Creates a new transaction item, or replaces and existing one if one exists with the specified id.
		/// </summary>
		/// <param name="transactionId">The id of the transaction to which to add a new item.</param>
		/// <param name="model">The transaction item to create or replace.</param>
		/// <returns>The id of the created or replaced transaction item.</returns>
		/// <response code="200">An existing transaction item was replaced.</response>
		/// <response code="201">A new transaction item was created.</response>
		/// <response code="404">Transaction with the specified id was not found.</response>
		[HttpPut("{transactionId:guid}/Item")]
		[ProducesResponseType(Status200OK)]
		[ProducesResponseType(Status201Created)]
		[ProducesResponseType(Status404NotFound)]
		public async Task<ActionResult<Guid>> PutItem(
			Guid transactionId,
			[FromBody, BindRequired] TransactionItemCreationModel model)
		{
			if (await _repository.FindByIdAsync(transactionId) is null)
			{
				return NotFound();
			}

			if (model.Id is null)
			{
				return await CreateNewItemAsync(transactionId, model, ApplicationUser);
			}

			var existingItem = await _itemRepository.FindByIdAsync(model.Id.Value);
			return existingItem is null
				? await CreateNewItemAsync(transactionId, model, ApplicationUser)
				: await UpdateExistingItemAsync(transactionId, model, ApplicationUser);
		}

		/// <summary>
		/// Deletes the specified transaction item.
		/// </summary>
		/// <param name="id">The id of the transaction item to delete.</param>
		/// <returns><see cref="NoContentResult"/> if transaction item was deleted successfully, otherwise <see cref="NotFoundResult"/>.</returns>
		/// <response code="204">Transaction item was successfully deleted.</response>
		/// <response code="404">Transaction item with the specified id does not exist.</response>
		[HttpDelete("Item/{id:guid}")]
		public async Task<StatusCodeResult> DeleteItem(Guid id)
		{
			var deletedCount = await _itemRepository.DeleteAsync(id);
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

		private async Task<ActionResult<Guid>> CreateNewTransactionAsync(
			TransactionCreationModel creationModel,
			UserEntity user)
		{
			var transaction = Mapper.Map<TransactionEntity>(creationModel) with
			{
				OwnerId = user.Id, // todo
				CreatedByUserId = user.Id,
				ModifiedByUserId = user.Id,
				ImportedAt = creationModel.ImportHash is null ? null : DateTimeOffset.UtcNow,
				ValidatedAt = creationModel.Validated ? DateTimeOffset.UtcNow : null,
				ValidatedByUserId = creationModel.Validated ? user.Id : null,
			};

			var transactionId = await _unitOfWork.AddAsync(transaction);
			return CreatedAtAction(nameof(Get), new { id = transactionId }, transactionId);
		}

		private async Task<ActionResult<Guid>> UpdateExistingTransactionAsync(
			TransactionCreationModel creationModel,
			UserEntity user,
			TransactionEntity existingTransaction)
		{
			if (creationModel.Items!.Any(item => item.Id is null))
			{
				return NotFound();
			}

			if (creationModel.Items!.Any(item =>
				existingTransaction.Items.All(entity => entity.Id != item.Id!.Value)))
			{
				return NotFound("Existing item with not found by id");
			}

			var transaction = Mapper.Map<TransactionEntity>(creationModel);
			_ = await _unitOfWork.UpdateAsync(transaction, user);
			return Ok(creationModel.Id);
		}

		private async Task<ActionResult<Guid>> CreateNewItemAsync(
			Guid transactionId,
			TransactionItemCreationModel creationModel,
			UserEntity user)
		{
			var transactionItem = Mapper.Map<TransactionItemEntity>(creationModel) with
			{
				OwnerId = user.Id, // todo
				CreatedByUserId = user.Id,
				ModifiedByUserId = user.Id,
				TransactionId = transactionId,
			};

			var id = await _itemRepository.AddAsync(transactionItem);
			return CreatedAtAction(nameof(GetItem), new { id }, id);
		}

		private async Task<ActionResult<Guid>> UpdateExistingItemAsync(
			Guid transactionId,
			TransactionItemCreationModel creationModel,
			UserEntity user)
		{
			var transactionItem = Mapper.Map<TransactionItemEntity>(creationModel);
			transactionItem.ModifiedByUserId = user.Id;
			transactionItem.TransactionId = transactionId;

			await _itemRepository.UpdateAsync(transactionItem);
			return Ok(creationModel.Id);
		}
	}
}
