// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using AutoMapper;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Entities.Abstractions;
using Gnomeshade.Interfaces.WebApi.V1_0.Authorization;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gnomeshade.Interfaces.WebApi.V1_0;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[AuthorizeApplicationUser]
[Route("api/v{version:apiVersion}/[controller]")]
[SuppressMessage(
	"ReSharper",
	"AsyncConverter.ConfigureAwaitHighlighting",
	Justification = "ASP.NET Core doesn't have a SynchronizationContext")]
public abstract class FinanceControllerBase<TEntity, TModel> : ControllerBase
	where TEntity : class, IEntity
	where TModel : class
{
	private readonly ApplicationUserContext _applicationUserContext;

	protected FinanceControllerBase(ApplicationUserContext applicationUserContext, Mapper mapper)
	{
		_applicationUserContext = applicationUserContext;
		Mapper = mapper;
	}

	protected Mapper Mapper { get; }

	/// <summary>
	/// Gets the <see cref="UserEntity"/> associated with the executing action.
	/// </summary>
	protected UserEntity ApplicationUser => _applicationUserContext.User;

	/// <summary>
	/// Finds a <typeparamref name="TModel"/> by the specified <paramref name="selector"/>.
	/// </summary>
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

	protected TModel MapToModel(TEntity entity) => Mapper.Map<TModel>(entity);
}
