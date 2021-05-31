using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

using Tracking.Finance.Data.Models;
using Tracking.Finance.Data.Repositories;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Tracking.Finance.Interfaces.WebApi.v1_0.Transactions
{
	/// <summary>
	/// CRUD operations on transaction entity.
	/// </summary>
	[ApiController]
	[ApiVersion("1.0")]
	[Route("api/v{version:apiVersion}/[controller]")]
	public sealed class TransactionController : ControllerBase
	{
		private readonly TransactionRepository _repository;
		private readonly Mapper _mapper;
		private readonly ILogger<TransactionController> _logger;

		/// <summary>
		/// Initializes a new instance of the <see cref="TransactionController"/> class.
		/// </summary>
		/// <param name="repository">The repository for performing CRUD operations.</param>
		/// <param name="mapper">Repository entity and API model mapper.</param>
		/// <param name="logger">Logger for logging in the specified category.</param>
		public TransactionController(TransactionRepository repository, Mapper mapper, ILogger<TransactionController> logger)
		{
			_repository = repository;
			_mapper = mapper;
			_logger = logger;
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
		public async Task<ActionResult<IEnumerable<TransactionModel>>> Get(CancellationToken cancellationToken)
		{
			var transactions = await _repository.GetAllAsync(cancellationToken);
			return Ok(transactions.Select(transaction => _mapper.Map<TransactionModel>(transaction)));
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
			var transaction = _mapper.Map<Transaction>(creationModel);
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
		[HttpPut("{id}")]
		[ProducesResponseType(Status200OK)]
		[ProducesResponseType(Status201Created)]
		public async Task<ActionResult<int>> Put(int id, [FromBody, BindRequired] TransactionCreationModel model)
		{
			var transaction = _mapper.Map<Transaction>(model);
			var existing = await _repository.FindByIdAsync(id);
			if (existing is null)
			{
				var newId = await _repository.AddAsync(transaction);
				return CreatedAtAction(nameof(Get), new { id = newId }, newId);
			}

			transaction.Id = id;
			_ = await _repository.UpdateAsync(transaction);
			return Ok(id);
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
	}
}
