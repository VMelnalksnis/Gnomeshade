﻿// Copyright 2021 Valters Melnalksnis
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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

using Tracking.Finance.Data.Identity;
using Tracking.Finance.Data.Models;
using Tracking.Finance.Data.Repositories;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Tracking.Finance.Interfaces.WebApi.V1_0.Transactions
{
	/// <summary>
	/// CRUD operations on transaction entity.
	/// </summary>
	[ApiController]
	[ApiVersion("1.0")]
	[Authorize]
	[Route("api/v{version:apiVersion}/[controller]")]
	[SuppressMessage(
		"ReSharper",
		"AsyncConverter.ConfigureAwaitHighlighting",
		Justification = "ASP.NET Core doesn't have a SynchronizationContext")]
	public sealed class TransactionController : ControllerBase, IDisposable
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly UserRepository _userRepository;
		private readonly IDbConnection _dbConnection;
		private readonly TransactionRepository _repository;
		private readonly TransactionItemRepository _itemRepository;
		private readonly Mapper _mapper;
		private readonly ILogger<TransactionController> _logger;

		/// <summary>
		/// Initializes a new instance of the <see cref="TransactionController"/> class.
		/// </summary>
		///
		/// <param name="userManager">Identity user manager.</param>
		/// <param name="userRepository">Finance user repository.</param>
		/// <param name="dbConnection">Database connection for creating <see cref="IDbTransaction"/> for creating entities.</param>
		/// <param name="repository">The repository for performing CRUD operations on <see cref="Transaction"/>.</param>
		/// <param name="itemRepository">The repository for performing CRUD operations on <see cref="TransactionItem"/>.</param>
		/// <param name="mapper">Repository entity and API model mapper.</param>
		/// <param name="logger">Logger for logging in the specified category.</param>
		public TransactionController(
			UserManager<ApplicationUser> userManager,
			UserRepository userRepository,
			IDbConnection dbConnection,
			TransactionRepository repository,
			TransactionItemRepository itemRepository,
			Mapper mapper,
			ILogger<TransactionController> logger)
		{
			_userManager = userManager;
			_userRepository = userRepository;
			_dbConnection = dbConnection;
			_repository = repository;
			_itemRepository = itemRepository;
			_mapper = mapper;
			_logger = logger;
		}

		/// <summary>
		/// Gets a transaction by the specified id.
		/// </summary>
		///
		/// <param name="id">The id of the transaction to get.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
		///
		/// <returns><see cref="OkObjectResult"/> if transaction was found, otherwise <see cref="NotFoundResult"/>.</returns>
		/// <response code="200">Transaction with the specified id exists.</response>
		/// <response code="404">Transaction with the specified id does not exist.</response>
		[HttpGet("{id:guid}")]
		[ProducesResponseType(Status200OK)]
		[ProducesResponseType(typeof(ProblemDetails), Status404NotFound)] // todo modify schema
		public async Task<ActionResult<TransactionModel>> Get(Guid id, CancellationToken cancellationToken)
		{
			var transaction = await _repository.FindByIdAsync(id, cancellationToken);
			if (transaction is null)
			{
				return NotFound();
			}

			var transactionModel = await GetModel(transaction, cancellationToken);
			return Ok(transactionModel);
		}

		/// <summary>
		/// Gets all transactions.
		/// </summary>
		///
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
		///
		/// <returns><see cref="OkObjectResult"/> with the transactions.</returns>
		/// <response code="200">Successfully got all transactions.</response>
		[HttpGet]
		[ProducesResponseType(Status200OK)]
		public async Task<ActionResult<IEnumerable<TransactionModel>>> GetAll(CancellationToken cancellationToken)
		{
			var transactions = await _repository.GetAllAsync(cancellationToken);
			var transactionModels =
				transactions
					.Select(transaction => GetModel(transaction, cancellationToken).GetAwaiter().GetResult())
					.ToList();

			return Ok(transactionModels);
		}

		/// <summary>
		/// Creates a new transaction.
		/// </summary>
		///
		/// <param name="creationModel">The transaction that will be created.</param>
		///
		/// <returns><see cref="CreatedAtActionResult"/> with the id of transaction.</returns>
		/// <response code="201">Transaction was successfully created.</response>
		[HttpPost]
		[ProducesResponseType(Status201Created)]
		public async Task<ActionResult<int>> Create([FromBody, BindRequired] TransactionCreationModel creationModel)
		{
			var user = await GetCurrentUser();
			if (user is null)
			{
				return Unauthorized();
			}

			var transaction = _mapper.Map<Transaction>(creationModel) with
			{
				OwnerId = user.Id, // todo
				CreatedByUserId = user.Id,
				ModifiedByUserId = user.Id,
			};

			if (!(creationModel.Items?.Any() ?? false))
			{
				var id = await _repository.AddAsync(transaction);
				return CreatedAtAction(nameof(Get), new { id }, id);
			}

			_dbConnection.Open();
			using var dbTransaction = _dbConnection.BeginTransaction();
			try
			{
				var transactionId = await _repository.AddAsync(transaction, dbTransaction);
				foreach (var item in creationModel.Items)
				{
					var transactionItem = _mapper.Map<TransactionItem>(item) with
					{
						OwnerId = user.Id,
						CreatedByUserId = user.Id,
						ModifiedByUserId = user.Id,
						TransactionId = transactionId,
					};

					_ = await _itemRepository.AddAsync(transactionItem, dbTransaction);
				}

				dbTransaction.Commit();
				return CreatedAtAction(nameof(Get), new { id = transactionId }, transactionId);
			}
			catch (Exception exception)
			{
				_logger.LogWarning(exception, "Failed to create transaction");
				dbTransaction.Rollback();
				throw;
			}
		}

		/// <summary>
		/// Deletes the specified transaction.
		/// </summary>
		///
		/// <param name="id">The id of the transaction to delete.</param>
		///
		/// <returns><see cref="NoContentResult"/> if transaction was deleted successfully, otherwise <see cref="NotFoundResult"/>.</returns>
		/// <response code="204">Transaction was successfully deleted.</response>
		/// <response code="404">Transaction with the specified id does not exist.</response>
		[HttpDelete("{id:guid}")]
		[ProducesResponseType(Status204NoContent)]
		[ProducesResponseType(typeof(ProblemDetails), Status404NotFound)]
		public async Task<StatusCodeResult> Delete(Guid id)
		{
			var deletedCount = await _repository.DeleteAsync(id);
			return deletedCount switch
			{
				0 => NotFound(),
				1 => NoContent(),
				_ => HandleFailedDelete(deletedCount, id),
			};

			StatusCodeResult HandleFailedDelete(int count, Guid transactionId)
			{
				_logger.LogError("Deleted {DeletedCount} transactions by id {TransactionId}", count, transactionId);
				return StatusCode(Status500InternalServerError);
			}
		}

		[HttpGet("{transactionId:guid}/Item")]
		public async Task<ActionResult<List<TransactionItemModel>>> GetItems(
			Guid transactionId,
			CancellationToken cancellationToken)
		{
			var items = await _itemRepository.GetAllAsync(transactionId, cancellationToken);
			return items.Select(item => _mapper.Map<TransactionItemModel>(item)).ToList();
		}

		[HttpGet("Item/{id:guid}")]
		public async Task<ActionResult<TransactionItemModel>> GetItem(Guid id, CancellationToken cancellationToken)
		{
			var item = await _itemRepository.FindByIdAsync(id, cancellationToken);
			if (item is null)
			{
				return NotFound();
			}

			return _mapper.Map<TransactionItemModel>(item);
		}

		[HttpPost("{transactionId:guid}/Item")]
		public async Task<ActionResult<Guid>> CreateItem(
			Guid transactionId,
			[FromBody, BindRequired] TransactionItemCreationModel creationModel)
		{
			var user = await GetCurrentUser();
			if (user is null)
			{
				return Unauthorized();
			}

			if (await _repository.FindByIdAsync(transactionId) is null)
			{
				// todo validation
				return new BadRequestResult();
			}

			var transactionItem = _mapper.Map<TransactionItem>(creationModel) with
			{
				OwnerId = user.Id, // todo
				TransactionId = transactionId,
			};

			return await _itemRepository.AddAsync(transactionItem);
		}

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

		/// <inheritdoc/>
		public void Dispose()
		{
			_dbConnection.Dispose();
			_userManager.Dispose();
			_repository.Dispose();
			_itemRepository.Dispose();
			_userRepository.Dispose();
		}

		private async Task<User?> GetCurrentUser()
		{
			var identityUser = await _userManager.GetUserAsync(User);
			if (identityUser is null)
			{
				return null;
			}

			return await _userRepository.FindByIdAsync(new(identityUser.Id));
		}

		private async Task<TransactionModel> GetModel(Transaction transaction, CancellationToken cancellationToken)
		{
			var items = await _itemRepository.GetAllAsync(transaction.Id, cancellationToken);
			var itemModels = items.Select(item => _mapper.Map<TransactionItemModel>(item)).ToList();
			return _mapper.Map<TransactionModel>(transaction) with { Items = itemModels };
		}
	}
}
