// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using Gnomeshade.Data;
using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Entities.Abstractions;
using Gnomeshade.Data.Repositories;
using Gnomeshade.WebApi.Models;
using Gnomeshade.WebApi.OpenApi;
using Gnomeshade.WebApi.V1_0.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.WebApi.V1_0;

/// <summary>Base class for controllers with endpoints implementations.</summary>
/// <typeparam name="TRepository">The type of the repository for <typeparamref name="TEntity"/>.</typeparam>
/// <typeparam name="TEntity">The entity on which to perform operations on.</typeparam>
/// <typeparam name="TModel">The public API model.</typeparam>
/// <typeparam name="TCreation">The public API model for creation entities.</typeparam>
public abstract class CreatableBase<TRepository, TEntity, TModel, TCreation> : FinanceControllerBase<TEntity, TModel>
	where TRepository : Repository<TEntity>
	where TEntity : class, IEntity
	where TModel : class
	where TCreation : Creation
{
	/// <summary>Initializes a new instance of the <see cref="CreatableBase{TRepository, TEntity, TModel, TCreation}"/> class.</summary>
	/// <param name="applicationUserContext">Context for getting the current application user.</param>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="repository">The repository for performing CRUD operations on <typeparamref name="TEntity"/>.</param>
	/// <param name="dbConnection">Database connection for transaction management.</param>
	protected CreatableBase(
		ApplicationUserContext applicationUserContext,
		Mapper mapper,
		ILogger<CreatableBase<TRepository, TEntity, TModel, TCreation>> logger,
		TRepository repository,
		DbConnection dbConnection)
		: base(applicationUserContext, mapper, logger)
	{
		Repository = repository;
		DbConnection = dbConnection;
	}

	/// <summary>Gets the repository for managing <typeparamref name="TEntity"/>.</summary>
	protected TRepository Repository { get; }

	/// <summary>Gets the database connection for managing database transactions.</summary>
	protected DbConnection DbConnection { get; }

	/// <summary>Gets all entities.</summary>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>A collection of all entities.</returns>
	[HttpGet]
	public virtual async Task<List<TModel>> Get(CancellationToken cancellationToken)
	{
		var entities = await Repository.GetAllAsync(ApplicationUser.Id, cancellationToken);
		return entities.Select(MapToModel).ToList();
	}

	/// <summary>Gets the specified entity.</summary>
	/// <param name="id">The id of the entity to get.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>The entity with the specified id.</returns>
	[HttpGet("{id:guid}")]
	[ProducesStatus404NotFound]
	public virtual Task<ActionResult<TModel>> Get(Guid id, CancellationToken cancellationToken)
	{
		return Find(() => Repository.FindByIdAsync(id, ApplicationUser.Id, AccessLevel.Read, cancellationToken));
	}

	/// <summary>Creates a new entity.</summary>
	/// <param name="creation">Information for creating the entity.</param>
	/// <returns>The id of the created entity.</returns>
	[HttpPost]
	[ProducesResponseType(Status201Created)]
	[ProducesStatus409Conflict]
	public virtual Task<ActionResult> Post([FromBody] TCreation creation)
	{
		creation = creation with { OwnerId = creation.OwnerId ?? ApplicationUser.Id };
		return CreateNewAsync(Guid.NewGuid(), creation, ApplicationUser);
	}

	/// <summary>Creates a new entity or replaces an existing one, if one exists with the specified id.</summary>
	/// <param name="id">The id of the entity.</param>
	/// <param name="creation">The entity to create or replace.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	[HttpPut("{id:guid}")]
	[ProducesResponseType(typeof(Guid), Status201Created)]
	[ProducesResponseType(Status204NoContent)]
	[ProducesResponseType(Status403Forbidden)]
	[ProducesStatus409Conflict]
	public virtual async Task<ActionResult> Put(Guid id, [FromBody] TCreation creation)
	{
		creation = creation with { OwnerId = creation.OwnerId ?? ApplicationUser.Id };

		var existingEntity = await Repository.FindByIdAsync(id, ApplicationUser.Id, AccessLevel.Write);
		if (existingEntity is not null)
		{
			return await UpdateExistingAsync(id, creation, ApplicationUser);
		}

		var conflictingEntity = await Repository.FindByIdAsync(id);
		if (conflictingEntity is null)
		{
			return await CreateNewAsync(id, creation, ApplicationUser);
		}

		return Forbid();
	}

	/// <summary>Deletes the entity.</summary>
	/// <param name="id">The id of the entity to delete.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	[HttpDelete("{id:guid}")]
	public virtual async Task<StatusCodeResult> Delete(Guid id)
	{
		await using var dbTransaction = await DbConnection.OpenAndBeginTransaction();

		var existing = await Repository.FindByIdAsync(id, ApplicationUser.Id, dbTransaction, AccessLevel.Delete);
		if (existing is null)
		{
			return NotFound();
		}

		await Repository.DeleteAsync(id, ApplicationUser.Id, dbTransaction);
		await dbTransaction.CommitAsync();
		return NoContent();
	}

	/// <summary>Updates an existing <typeparamref name="TEntity"/> with details from <paramref name="creation"/>.</summary>
	/// <param name="id">The id of the entity to update.</param>
	/// <param name="creation">The details to update.</param>
	/// <param name="user">The current user.</param>
	/// <returns>An action result.</returns>
	protected abstract Task<ActionResult> UpdateExistingAsync(Guid id, TCreation creation, UserEntity user);

	/// <summary>Creates a new <typeparamref name="TEntity"/> with details from <paramref name="creation"/>.</summary>
	/// <param name="id">The id of the entity to create.</param>
	/// <param name="creation">The details from which to create the entity.</param>
	/// <param name="user">The current user.</param>
	/// <returns>An action result.</returns>
	protected abstract Task<ActionResult> CreateNewAsync(Guid id, TCreation creation, UserEntity user);
}
