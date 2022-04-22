// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;
using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Tags;
using Gnomeshade.Interfaces.WebApi.OpenApi;
using Gnomeshade.Interfaces.WebApi.V1_0.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.Interfaces.WebApi.V1_0.Categories;

/// <summary>CRUD operations on categories.</summary>
public sealed class CategoriesController : FinanceControllerBase<CategoryEntity, Category>
{
	private readonly CategoryRepository _repository;

	/// <summary>Initializes a new instance of the <see cref="CategoriesController"/> class.</summary>
	/// <param name="applicationUserContext">Context for getting the current application user.</param>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="repository">The repository for performing CRUD operations on <see cref="CategoryEntity"/>.</param>
	public CategoriesController(
		ApplicationUserContext applicationUserContext,
		Mapper mapper,
		ILogger<CategoriesController> logger,
		CategoryRepository repository)
		: base(applicationUserContext, mapper, logger)
	{
		_repository = repository;
	}

	/// <inheritdoc cref="ICategoryClient.GetCategoriesAsync"/>
	/// <response code="200">Successfully got the categories.</response>
	[HttpGet]
	[ProducesResponseType(typeof(List<Category>), Status200OK)]
	public async Task<ActionResult<List<Category>>> Get(CancellationToken cancellationToken = default)
	{
		var tagEntities = await _repository.GetAllAsync(ApplicationUser.Id, cancellationToken);
		var categories = tagEntities.Select(MapToModel).ToList();
		return Ok(categories);
	}

	/// <inheritdoc cref="ICategoryClient.GetCategoryAsync"/>
	/// <response code="200">Successfully got the tag.</response>
	/// <response code="404">Tag with the specified id does not exist.</response>
	[HttpGet("{id:guid}")]
	[ProducesResponseType(typeof(Category), Status200OK)]
	[ProducesStatus404NotFound]
	public async Task<ActionResult<Category>> Get(Guid id, CancellationToken cancellationToken = default)
	{
		return await Find(() => _repository.FindByIdAsync(id, ApplicationUser.Id, cancellationToken));
	}

	/// <inheritdoc cref="ICategoryClient.PutCategoryAsync"/>
	/// <response code="201">A new product was created.</response>
	/// <response code="409">A product with the specified name already exists.</response>
	[HttpPost]
	[ProducesResponseType(Status201Created)]
	[ProducesStatus409Conflict]
	public async Task<ActionResult<Guid>> Post([FromBody] CategoryCreation category)
	{
		return await CreateTagAsync(category, ApplicationUser, Guid.NewGuid());
	}

	/// <inheritdoc cref="ICategoryClient.PutCategoryAsync"/>
	/// <response code="201">A new product was created.</response>
	/// <response code="204">An existing product was replaced.</response>
	/// <response code="409">A product with the specified name already exists.</response>
	[HttpPut("{id:guid}")]
	[ProducesResponseType(Status201Created)]
	[ProducesResponseType(Status204NoContent)]
	[ProducesStatus409Conflict]
	public async Task<ActionResult> Put(Guid id, [FromBody] CategoryCreation category)
	{
		var existingTag = await _repository.FindByIdAsync(id, ApplicationUser.Id);
		return existingTag is null
			? await CreateTagAsync(category, ApplicationUser, id)
			: await UpdateTagAsync(category, ApplicationUser, id);
	}

	/// <inheritdoc cref="ICategoryClient.DeleteCategoryAsync"/>
	/// <response code="204">The tag was deleted successfully.</response>
	[HttpDelete("{id:guid}")]
	public async Task<StatusCodeResult> Delete(Guid id)
	{
		var deletedCount = await _repository.DeleteAsync(id, ApplicationUser.Id);
		return DeletedEntity<CategoryEntity>(id, deletedCount);
	}

	private async Task<ActionResult> CreateTagAsync(CategoryCreation model, UserEntity user, Guid id)
	{
		var normalizedName = model.Name.ToUpperInvariant();
		var conflictingTag = await _repository.FindByNameAsync(normalizedName, user.Id);
		if (conflictingTag is not null)
		{
			return Problem(
				"Tag with the specified name already exists",
				Url.Action(nameof(Get), new { conflictingTag.Id }),
				Status409Conflict);
		}

		var tag = Mapper.Map<CategoryEntity>(model) with
		{
			Id = id,
			OwnerId = user.Id,
			CreatedByUserId = user.Id,
			ModifiedByUserId = user.Id,
			NormalizedName = normalizedName,
		};

		_ = await _repository.AddAsync(tag);
		return CreatedAtAction(nameof(Get), new { id }, id);
	}

	private async Task<NoContentResult> UpdateTagAsync(CategoryCreation model, UserEntity user, Guid id)
	{
		var tag = Mapper.Map<CategoryEntity>(model) with
		{
			Id = id,
			OwnerId = user.Id, // todo only works for entities created by the user
			NormalizedName = model.Name.ToUpperInvariant(),
			ModifiedByUserId = user.Id,
		};

		var x = await _repository.UpdateAsync(tag);
		Debug.Assert(x > 0, "No rows were changed after update");
		return NoContent();
	}
}
