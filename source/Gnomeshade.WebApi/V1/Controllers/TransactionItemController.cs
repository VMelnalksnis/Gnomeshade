// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data.Common;
using System.Threading.Tasks;

using AutoMapper;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Entities.Abstractions;
using Gnomeshade.Data.Repositories;
using Gnomeshade.WebApi.Models.Transactions;

using Microsoft.AspNetCore.Mvc;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.WebApi.V1.Controllers;

/// <summary>Controller for sub-items of <see cref="Transaction"/>.</summary>
/// <typeparam name="TRepository">The type of the repository for <typeparamref name="TEntity"/>.</typeparam>
/// <typeparam name="TEntity">The entity on which to perform operations on.</typeparam>
/// <typeparam name="TModel">The public API model.</typeparam>
/// <typeparam name="TItemCreation">The public API model for creation entities.</typeparam>
public abstract class TransactionItemController<TRepository, TEntity, TModel, TItemCreation>
	: CreatableBase<TRepository, TEntity, TModel, TItemCreation>
	where TRepository : Repository<TEntity>
	where TEntity : Entity, IModifiableEntity
	where TModel : class
	where TItemCreation : TransactionItemCreation
{
	private readonly TransactionRepository _transactionRepository;

	/// <summary>Initializes a new instance of the <see cref="TransactionItemController{TRepository, TEntity, TModel, TCreation}"/> class.</summary>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	/// <param name="repository">The repository for performing CRUD operations on <typeparamref name="TEntity"/>.</param>
	/// <param name="dbConnection">Database connection for transaction management.</param>
	/// <param name="transactionRepository">Transaction repository for checking conflicts.</param>
	protected TransactionItemController(
		Mapper mapper,
		TRepository repository,
		DbConnection dbConnection,
		TransactionRepository transactionRepository)
		: base(mapper, repository, dbConnection)
	{
		_transactionRepository = transactionRepository;
	}

	/// <inheritdoc />
	protected sealed override async Task<ActionResult> UpdateExistingAsync(
		Guid id,
		TItemCreation creation,
		UserEntity user,
		DbTransaction dbTransaction)
	{
		var conflictingResult = await FindConflictingTransaction(creation.TransactionId!.Value, user.Id, dbTransaction);
		if (conflictingResult is not null)
		{
			return conflictingResult;
		}

		var entity = Mapper.Map<TEntity>(creation) with
		{
			Id = id,
			CreatedByUserId = ApplicationUser.Id,
			ModifiedByUserId = ApplicationUser.Id,
		};

		if (await Repository.UpdateAsync(entity, dbTransaction) is not 1)
		{
			return StatusCode(Status403Forbidden);
		}

		return NoContent();
	}

	/// <inheritdoc />
	protected sealed override async Task<ActionResult> CreateNewAsync(
		Guid id,
		TItemCreation creation,
		UserEntity user,
		DbTransaction dbTransaction)
	{
		var conflictingResult = await FindConflictingTransaction(creation.TransactionId!.Value, user.Id, dbTransaction);
		if (conflictingResult is not null)
		{
			return conflictingResult;
		}

		var entity = Mapper.Map<TEntity>(creation) with
		{
			Id = id,
			CreatedByUserId = ApplicationUser.Id,
			ModifiedByUserId = ApplicationUser.Id,
		};

		await Repository.AddAsync(entity, dbTransaction);
		return CreatedAtAction(nameof(Get), new { id }, id);
	}

	private async Task<ActionResult?> FindConflictingTransaction(
		Guid transactionId,
		Guid userId,
		DbTransaction dbTransaction)
	{
		var transaction = await _transactionRepository.FindByIdAsync(transactionId, userId, dbTransaction, AccessLevel.Write);
		if (transaction is not null)
		{
			return null;
		}

		return await _transactionRepository.FindByIdAsync(transactionId, dbTransaction) is null
			? NotFound()
			: StatusCode(Status403Forbidden);
	}
}
