// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using AutoMapper;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Entities.Abstractions;
using Gnomeshade.Interfaces.WebApi.V1_0.Authorization;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.Interfaces.WebApi.V1_0;

/// <summary>Base class for controllers handling Gnomeshade specific entities.</summary>
/// <typeparam name="TEntity">The type of the database model.</typeparam>
/// <typeparam name="TModel">The type of the public API model.</typeparam>
[ApiController]
[ApiVersion("1.0")]
[Authorize]
[AuthorizeApplicationUser]
[Route("api/v{version:apiVersion}/[controller]")]
public abstract class FinanceControllerBase<TEntity, TModel> : ControllerBase
	where TEntity : class, IEntity
	where TModel : class
{
	private readonly ApplicationUserContext _applicationUserContext;
	private readonly ILogger<FinanceControllerBase<TEntity, TModel>> _logger;

	/// <summary>Initializes a new instance of the <see cref="FinanceControllerBase{TEntity, TModel}"/> class.</summary>
	/// <param name="applicationUserContext">Context for getting the current application user.</param>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	/// <param name="logger">Logger for logging in the specified category.</param>
	protected FinanceControllerBase(
		ApplicationUserContext applicationUserContext,
		Mapper mapper,
		ILogger<FinanceControllerBase<TEntity, TModel>> logger)
	{
		_applicationUserContext = applicationUserContext;
		Mapper = mapper;
		_logger = logger;
	}

	/// <summary>Gets the repository entity and API model mapper.</summary>
	protected Mapper Mapper { get; }

	/// <summary>Gets the <see cref="UserEntity"/> associated with the executing action.</summary>
	protected UserEntity ApplicationUser => _applicationUserContext.User;

	/// <summary>Finds a <typeparamref name="TModel"/> by the specified <paramref name="selector"/>.</summary>
	/// <param name="selector">Asynchronous function for finding an instance of <typeparamref name="TEntity"/>.</param>
	/// <returns><see cref="OkObjectResult"/> if an instance of <typeparamref name="TModel"/> was found, otherwise <see cref="NotFoundResult"/>.</returns>
	protected async Task<ActionResult<TModel>> Find(Func<Task<TEntity?>> selector)
	{
		var entity = await selector();
		if (entity is null)
		{
			return NotFound();
		}

		var model = MapToModel(entity);
		return Ok(model);
	}

	/// <summary>Maps a repository entity to an API model.</summary>
	/// <param name="entity">The repository entity to map.</param>
	/// <returns>An API model equivalent to <paramref name="entity"/>.</returns>
	protected TModel MapToModel(TEntity entity) => Mapper.Map<TModel>(entity);

	/// <summary>Creates an action result for entity deletion.</summary>
	/// <param name="id">The id of the deleted entity.</param>
	/// <param name="deletedCount">The count of deleted rows.</param>
	/// <typeparam name="TDeleted">The type of the deleted entity.</typeparam>
	/// <returns>Appropriate action result.</returns>
	protected StatusCodeResult DeletedEntity<TDeleted>(Guid id, int deletedCount)
	{
		return deletedCount switch
		{
			0 => NotFound(),
			1 => NoContent(),
			_ => HandleFailedDelete(deletedCount, id),
		};

		StatusCodeResult HandleFailedDelete(int count, Guid transferId)
		{
			_logger.LogError(
				"Deleted {EntityCount} {EntityType} by id {EntityId}",
				count,
				typeof(TDeleted).Name,
				transferId);

			return StatusCode(Status500InternalServerError);
		}
	}
}
