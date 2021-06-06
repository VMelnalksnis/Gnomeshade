﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU General Public License 3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
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
	public sealed class TransactionController : ControllerBase
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly UserRepository _userRepository;
		private readonly TransactionRepository _repository;
		private readonly TransactionItemRepository _itemRepository;
		private readonly Mapper _mapper;
		private readonly ILogger<TransactionController> _logger;

		/// <summary>
		/// Initializes a new instance of the <see cref="TransactionController"/> class.
		/// </summary>
		/// <param name="repository">The repository for performing CRUD operations.</param>
		/// <param name="mapper">Repository entity and API model mapper.</param>
		/// <param name="logger">Logger for logging in the specified category.</param>
		public TransactionController(
			UserManager<ApplicationUser> userManager,
			UserRepository userRepository,
			TransactionRepository repository,
			TransactionItemRepository itemRepository,
			Mapper mapper,
			ILogger<TransactionController> logger)
		{
			_userManager = userManager;
			_userRepository = userRepository;
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
		[HttpGet("{id}")]
		[ProducesResponseType(Status200OK)]
		[ProducesResponseType(typeof(ProblemDetails), Status404NotFound)] // todo modify schema
		public async Task<ActionResult<TransactionModel>> Get(int id, CancellationToken cancellationToken)
		{
			var transaction = await _repository.FindByIdAsync(id, cancellationToken);
			if (transaction is null)
			{
				return NotFound();
			}

			return Ok(_mapper.Map<TransactionModel>(transaction));
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
			return Ok(transactions.Select(transaction => _mapper.Map<TransactionModel>(transaction)));
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
			var identityUser = await _userManager.GetUserAsync(User);
			var user = (await _userRepository.GetAllAsync()).Single(u => u.IdentityUserId == identityUser.Id);

			var transaction = _mapper.Map<Transaction>(creationModel);
			transaction.UserId = user.Id;
			transaction.CreatedByUserId = user.Id;
			transaction.ModifiedByUserId = user.Id;
			var currentTime = DateTimeOffset.Now;
			transaction.CreatedAt = currentTime;
			transaction.ModifiedAt = currentTime;

			var id = await _repository.AddAsync(transaction);
			return CreatedAtAction(nameof(Get), new { id }, id);
		}

		/// <summary>
		/// Updates a transaction if one exists with the specified id, otherwise creates a new transaction.
		/// </summary>
		///
		/// <param name="id">The id of the transaction to update.</param>
		/// <param name="model">The transaction that will be updated/created.</param>
		///
		/// <returns><see cref="OkObjectResult"/> if an existing transaction was updated or <see cref="CreatedAtActionResult"/> if a new one was created.</returns>
		/// <response code="200">An existing transaction was updated with the specified values.</response>
		/// <response code="201">An new transaction was created.</response>
		//[HttpPut("{id}")]
		//[ProducesResponseType(Status200OK)]
		//[ProducesResponseType(Status201Created)]
		//public async Task<ActionResult<int>> Put(int id, [FromBody, BindRequired] TransactionCreationModel model)
		//{
		//	var transaction = _mapper.Map<Transaction>(model);
		//	var existing = await _repository.FindByIdAsync(id);
		//	if (existing is null)
		//	{
		//		var newId = await _repository.AddAsync(transaction);
		//		return CreatedAtAction(nameof(Get), new { id = newId }, newId);
		//	}

		//	transaction.Id = id;
		//	await _repository.UpdateAsync(transaction);
		//	return Ok(id);
		//}

		/// <summary>
		/// Deletes the specified transaction.
		/// </summary>
		///
		/// <param name="id">The id of the transaction to delete.</param>
		///
		/// <returns><see cref="NoContentResult"/> if transaction was deleted successfully, otherwise <see cref="NotFoundResult"/>.</returns>
		/// <response code="204">Transaction was successfully deleted.</response>
		/// <response code="404">Transaction with the specified id does not exist.</response>
		[HttpDelete("{id}")]
		[ProducesResponseType(Status204NoContent)]
		[ProducesResponseType(typeof(ProblemDetails), Status404NotFound)]
		public async Task<StatusCodeResult> Delete(int id)
		{
			var deletedCount = await _repository.DeleteAsync(id);
			return deletedCount switch
			{
				0 => NotFound(),
				1 => NoContent(),
				_ => HandleFailedDelete(deletedCount, id),
			};

			StatusCodeResult HandleFailedDelete(int deletedCount, int transactionId)
			{
				_logger.LogError("Deleted {DeletedCount} transactions by id {TransactionId}", deletedCount, transactionId);
				return StatusCode(Status500InternalServerError);
			}
		}

		[HttpGet("{transactionId}/Item")]
		public async Task<ActionResult<List<TransactionItemModel>>> GetItems(int transactionId, CancellationToken cancellationToken)
		{
			var items = await _itemRepository.GetAllAsync(transactionId, cancellationToken);
			return items.Select(item => _mapper.Map<TransactionItemModel>(item)).ToList();
		}

		[HttpGet("Item/{id}")]
		public async Task<ActionResult<TransactionItemModel>> GetItem(int id, CancellationToken cancellationToken)
		{
			var item = await _itemRepository.FindByIdAsync(id, cancellationToken);
			if (item is null)
			{
				return NotFound();
			}

			return _mapper.Map<TransactionItemModel>(item);
		}

		[HttpPost("{transactionId}/Item")]
		public async Task<ActionResult<int>> CreateItem(int transactionId, [FromBody, BindRequired] TransactionItemCreationModel creationModel)
		{
			if (await _repository.FindByIdAsync(transactionId) is null)
			{
				// todo validation
				return new BadRequestResult();
			}

			var transactionItem = _mapper.Map<TransactionItem>(creationModel);
			transactionItem.TransactionId = transactionId;
			return await _itemRepository.AddAsync(transactionItem);
		}

		[HttpDelete("Item/{id}")]
		public async Task<StatusCodeResult> DeleteItem(int id)
		{
			var deletedCount = await _itemRepository.DeleteAsync(id);
			return deletedCount switch
			{
				0 => NotFound(),
				1 => NoContent(),
				_ => HandleFailedDelete(deletedCount, id),
			};

			StatusCodeResult HandleFailedDelete(int deletedCount, int transactionItemId)
			{
				_logger.LogError("Deleted {DeletedCount} transaction items by id {TransactionItemId}", deletedCount, transactionItemId);
				return StatusCode(Status500InternalServerError);
			}
		}
	}
}
