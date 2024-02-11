// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using AutoMapper;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Entities.Abstractions;
using Gnomeshade.WebApi.V1.Authentication;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using static System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes;

namespace Gnomeshade.WebApi.V1;

/// <summary>Base class for controllers handling Gnomeshade specific entities.</summary>
/// <typeparam name="TEntity">The type of the database model.</typeparam>
/// <typeparam name="TModel">The type of the public API model.</typeparam>
[ApiController]
[Authorize]
[Route("api/v{version:apiVersion}/[controller]")]
public abstract class FinanceControllerBase<[DynamicallyAccessedMembers(All)] TEntity, [DynamicallyAccessedMembers(All)] TModel> : ControllerBase
	where TEntity : class, IEntity
	where TModel : class
{
	/// <summary>Initializes a new instance of the <see cref="FinanceControllerBase{TEntity, TModel}"/> class.</summary>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	protected FinanceControllerBase(Mapper mapper)
	{
		Mapper = mapper;
	}

	/// <summary>Gets the repository entity and API model mapper.</summary>
	protected Mapper Mapper { get; }

	/// <summary>Gets the <see cref="UserEntity"/> associated with the executing action.</summary>
	protected UserEntity ApplicationUser => User.ToApplicationUser();

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
	protected TModel MapToModel(TEntity entity)
	{
		return Mapper.Map<TModel>(entity);
	}
}
