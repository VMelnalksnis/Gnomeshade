// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using Gnomeshade.Data;
using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Identity;
using Gnomeshade.Data.Repositories;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;

using Microsoft.AspNetCore.Identity;
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
		private readonly IDbConnection _dbConnection;
		private readonly TransactionRepository _repository;
		private readonly TransactionItemRepository _itemRepository;
		private readonly ILogger<TransactionController> _logger;
		private readonly TransactionUnitOfWork _unitOfWork;

		/// <summary>
		/// Initializes a new instance of the <see cref="TransactionController"/> class.
		/// </summary>
		/// <param name="userManager">Identity user manager.</param>
		/// <param name="userRepository">Finance user repository.</param>
		/// <param name="dbConnection">Database connection for creating <see cref="IDbTransaction"/> for creating entities.</param>
		/// <param name="repository">The repository for performing CRUD operations on <see cref="TransactionEntity"/>.</param>
		/// <param name="itemRepository">The repository for performing CRUD operations on <see cref="TransactionItemEntity"/>.</param>
		/// <param name="mapper">Repository entity and API model mapper.</param>
		/// <param name="logger">Logger for logging in the specified category.</param>
		/// <param name="unitOfWork">Unit of work for managing transactions and all related entities.</param>
		public TransactionController(
			UserManager<ApplicationUser> userManager,
			UserRepository userRepository,
			IDbConnection dbConnection,
			TransactionRepository repository,
			TransactionItemRepository itemRepository,
			Mapper mapper,
			ILogger<TransactionController> logger,
			TransactionUnitOfWork unitOfWork)
			: base(userManager, userRepository, mapper)
		{
			_dbConnection = dbConnection;
			_repository = repository;
			_itemRepository = itemRepository;
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
			var (fromDate, toDate) = TimeRange.FromOptional(timeRange, DateTimeOffset.Now);

			var transactions = await _repository.GetAllAsync(fromDate, toDate, cancellation);
			var transactionModels = transactions.Select(MapToModel).ToList();
			return Ok(transactionModels);
		}

		/// <summary>
		/// Creates a new transaction.
		/// </summary>
		/// <param name="creationModel">The transaction that will be created.</param>
		/// <returns><see cref="CreatedAtActionResult"/> with the id of the transaction.</returns>
		/// <response code="201">Transaction was successfully created.</response>
		[HttpPost]
		[ProducesResponseType(Status201Created)]
		public async Task<ActionResult<Guid>> Create([FromBody, BindRequired] TransactionCreationModel creationModel)
		{
			var user = await GetCurrentUser();
			if (user is null)
			{
				return Unauthorized();
			}

			var transaction = Mapper.Map<TransactionEntity>(creationModel) with
			{
				OwnerId = user.Id, // todo
				CreatedByUserId = user.Id,
				ModifiedByUserId = user.Id,
				ImportedAt = creationModel.ImportHash is null ? null : DateTimeOffset.Now,
				ValidatedAt = creationModel.Validated ? DateTimeOffset.Now : null,
				ValidatedByUserId = creationModel.Validated ? user.Id : null,
			};

			var transactionId = await _unitOfWork.AddAsync(transaction);
			return CreatedAtAction(nameof(Get), new { id = transactionId }, transactionId);
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
			var item = await _itemRepository.FindByIdAsync(id, cancellation);
			if (item is null)
			{
				return NotFound();
			}

			return Mapper.Map<TransactionItem>(item);
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
			var user = await GetCurrentUser();
			if (user is null)
			{
				return Unauthorized();
			}

			if (await _repository.FindByIdAsync(transactionId) is null)
			{
				return NotFound();
			}

			if (model.Id is null)
			{
				return await CreateNewItemAsync(transactionId, model, user);
			}

			var existingItem = await _itemRepository.FindByIdAsync(model.Id.Value);
			return existingItem is null
				? await CreateNewItemAsync(transactionId, model, user)
				: await UpdateExistingItemAsync(transactionId, model, user);
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

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			if (!disposing)
			{
				return;
			}

			_dbConnection.Dispose();
			_repository.Dispose();
			_itemRepository.Dispose();
			base.Dispose(disposing);
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

			var id = await _itemRepository.UpdateAsync(transactionItem);
			return Ok(id);
		}
	}
}
