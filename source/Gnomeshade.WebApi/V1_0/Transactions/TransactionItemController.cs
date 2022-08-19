// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data;
using System.Threading.Tasks;

using AutoMapper;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Entities.Abstractions;
using Gnomeshade.Data.Repositories;
using Gnomeshade.WebApi.Models.Transactions;
using Gnomeshade.WebApi.V1_0.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Gnomeshade.WebApi.V1_0.Transactions;

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
	/// <param name="applicationUserContext">Context for getting the current application user.</param>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="repository">The repository for performing CRUD operations on <typeparamref name="TEntity"/>.</param>
	/// <param name="dbConnection">Database connection for transaction management.</param>
	/// <param name="transactionRepository">Transaction repository for checking conflicts.</param>
	protected TransactionItemController(
		ApplicationUserContext applicationUserContext,
		Mapper mapper,
		ILogger<TransactionItemController<TRepository, TEntity, TModel, TItemCreation>> logger,
		TRepository repository,
		IDbConnection dbConnection,
		TransactionRepository transactionRepository)
		: base(applicationUserContext, mapper, logger, repository, dbConnection)
	{
		_transactionRepository = transactionRepository;
	}

	/// <inheritdoc />
	protected sealed override async Task<ActionResult> UpdateExistingAsync(Guid id, TItemCreation creation, UserEntity user)
	{
		var dbTransaction = DbConnection.BeginTransaction();
		try
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

			await Repository.UpdateAsync(entity, dbTransaction);
			dbTransaction.Commit();
			return NoContent();
		}
		catch (Exception)
		{
			dbTransaction.Rollback();
			throw;
		}
	}

	/// <inheritdoc />
	protected sealed override async Task<ActionResult> CreateNewAsync(Guid id, TItemCreation creation, UserEntity user)
	{
		var dbTransaction = DbConnection.BeginTransaction();
		try
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
			dbTransaction.Commit();
			return CreatedAtAction("Get", new { id }, null);
		}
		catch (Exception)
		{
			dbTransaction.Rollback();
			throw;
		}
	}

	private async Task<ActionResult?> FindConflictingTransaction(
		Guid transactionId,
		Guid userId,
		IDbTransaction dbTransaction)
	{
		var transaction = await _transactionRepository.FindByIdAsync(transactionId, userId, dbTransaction, AccessLevel.Write);
		if (transaction is not null)
		{
			return null;
		}

		return await _transactionRepository.FindByIdAsync(transactionId, dbTransaction) is null
			? NotFound()
			: Forbid();
	}
}
