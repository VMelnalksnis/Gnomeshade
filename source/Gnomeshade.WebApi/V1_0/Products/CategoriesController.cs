// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Products;
using Gnomeshade.WebApi.V1_0.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.WebApi.V1_0.Products;

/// <summary>CRUD operations on categories.</summary>
public sealed class CategoriesController : CreatableBase<CategoryRepository, CategoryEntity, Category, CategoryCreation>
{
	/// <summary>Initializes a new instance of the <see cref="CategoriesController"/> class.</summary>
	/// <param name="applicationUserContext">Context for getting the current application user.</param>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="repository">The repository for performing CRUD operations on <see cref="CategoryEntity"/>.</param>
	/// <param name="dbConnection">Database connection for transaction management.</param>
	public CategoriesController(
		ApplicationUserContext applicationUserContext,
		Mapper mapper,
		ILogger<CategoriesController> logger,
		CategoryRepository repository,
		IDbConnection dbConnection)
		: base(applicationUserContext, mapper, logger, repository, dbConnection)
	{
	}

	/// <inheritdoc cref="IProductClient.GetCategoriesAsync"/>
	/// <response code="200">Successfully got the categories.</response>
	[HttpGet]
	[ProducesResponseType(typeof(List<Category>), Status200OK)]
	public override Task<List<Category>> Get(CancellationToken cancellationToken) =>
		base.Get(cancellationToken);

	/// <inheritdoc cref="IProductClient.GetCategoryAsync"/>
	/// <response code="200">Successfully got the category.</response>
	/// <response code="404">Category with the specified id does not exist.</response>
	[ProducesResponseType(typeof(Category), Status200OK)]
	public override Task<ActionResult<Category>> Get(Guid id, CancellationToken cancellationToken) =>
		base.Get(id, cancellationToken);

	/// <inheritdoc cref="IProductClient.PutCategoryAsync"/>
	/// <response code="201">A new category was created.</response>
	/// <response code="409">A category with the specified name already exists.</response>
	public override Task<ActionResult> Post([FromBody] CategoryCreation category) =>
		base.Post(category);

	/// <inheritdoc cref="IProductClient.PutCategoryAsync"/>
	/// <response code="201">A new category was created.</response>
	/// <response code="204">An existing category was replaced.</response>
	/// <response code="409">A category with the specified name already exists.</response>
	public override Task<ActionResult> Put(Guid id, [FromBody] CategoryCreation category) =>
		base.Put(id, category);

	/// <inheritdoc cref="IProductClient.DeleteCategoryAsync"/>
	/// <response code="204">The category was deleted successfully.</response>
	// ReSharper disable once RedundantOverriddenMember
	public override Task<StatusCodeResult> Delete(Guid id) =>
		base.Delete(id);

	/// <inheritdoc />
	protected override async Task<ActionResult> UpdateExistingAsync(Guid id, CategoryCreation creation, UserEntity user)
	{
		var category = Mapper.Map<CategoryEntity>(creation) with
		{
			Id = id,
			ModifiedByUserId = user.Id,
		};

		await Repository.UpdateAsync(category);
		return NoContent();
	}

	/// <inheritdoc />
	protected override async Task<ActionResult> CreateNewAsync(Guid id, CategoryCreation creation, UserEntity user)
	{
		var conflictingCategory = await Repository.FindByNameAsync(creation.Name, user.Id);
		if (conflictingCategory is not null)
		{
			return Problem(
				$"{nameof(Category)} with the specified name already exists",
				Url.Action(nameof(Get), new { conflictingCategory.Id }),
				Status409Conflict);
		}

		var category = Mapper.Map<CategoryEntity>(creation) with
		{
			Id = id,
			CreatedByUserId = user.Id,
			ModifiedByUserId = user.Id,
		};

		_ = await Repository.AddAsync(category);
		return CreatedAtAction(nameof(Get), new { id }, id);
	}
}
