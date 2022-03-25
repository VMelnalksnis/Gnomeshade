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
using Gnomeshade.Interfaces.WebApi.Models.Products;
using Gnomeshade.Interfaces.WebApi.OpenApi;
using Gnomeshade.Interfaces.WebApi.V1_0.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.Interfaces.WebApi.V1_0.Products;

/// <summary>
/// CRUD operations on account entity.
/// </summary>
public sealed class ProductsController : FinanceControllerBase<ProductEntity, Product>
{
	private readonly ProductRepository _repository;

	/// <summary>
	/// Initializes a new instance of the <see cref="ProductsController"/> class.
	/// </summary>
	/// <param name="repository">The repository for performing CRUD operations on <see cref="ProductEntity"/>.</param>
	/// <param name="applicationUserContext">Context for getting the current application user.</param>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	public ProductsController(
		ProductRepository repository,
		ApplicationUserContext applicationUserContext,
		Mapper mapper)
		: base(applicationUserContext, mapper)
	{
		_repository = repository;
	}

	/// <summary>
	/// Gets the product with the specified id.
	/// </summary>
	/// <param name="id">The id of the product.</param>
	/// <param name="cancellation">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>The product with the specified id.</returns>
	/// <response code="200">Successfully got the product.</response>
	/// <response code="404">Product with the specified id does not exist.</response>
	[HttpGet("{id:guid}")]
	[ProducesStatus404NotFound]
	public async Task<ActionResult<Product>> Get(Guid id, CancellationToken cancellation)
	{
		return await Find(() => _repository.FindByIdAsync(id, ApplicationUser.Id, cancellation));
	}

	/// <summary>
	/// Gets all products.
	/// </summary>
	/// <param name="cancellation">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>A collection of all products.</returns>
	[HttpGet]
	public async Task<List<Product>> GetAll(CancellationToken cancellation)
	{
		var products = await _repository.GetAllAsync(ApplicationUser.Id, cancellation);

		// ReSharper disable once ConvertClosureToMethodGroup
		return products.Select(account => MapToModel(account)).ToList();
	}

	/// <summary>
	/// Creates a new product, or replaces and existing one if one exists with the specified id.
	/// </summary>
	/// <param name="id">The id of the product.</param>
	/// <param name="model">The product to create or replace.</param>
	/// <returns>A status code indicating the result of the action.</returns>
	/// <response code="201">A new product was created.</response>
	/// <response code="204">An existing product was replaced.</response>
	/// <response code="409">A product with the specified name already exists.</response>
	[HttpPut("{id:guid}")]
	[ProducesResponseType(Status201Created)]
	[ProducesResponseType(Status204NoContent)]
	[ProducesStatus409Conflict]
	public async Task<ActionResult> Put(Guid id, [FromBody, BindRequired] ProductCreationModel model)
	{
		var existingProduct = await _repository.FindByIdAsync(id, ApplicationUser.Id);
		return existingProduct is null
			? await PutNewProductAsync(model, ApplicationUser, id)
			: await UpdateExistingProductAsync(model, ApplicationUser, id);
	}

	private async Task<ActionResult> PutNewProductAsync(ProductCreationModel model, UserEntity user, Guid id)
	{
		var normalizedName = model.Name!.ToUpperInvariant();
		var conflictingProduct = await _repository.FindByNameAsync(normalizedName, user.Id);
		if (conflictingProduct is not null)
		{
			return Problem(
				"Product with the specified name already exists",
				Url.Action(nameof(Get), new { conflictingProduct.Id }),
				Status409Conflict);
		}

		var product = Mapper.Map<ProductEntity>(model) with
		{
			Id = id,
			OwnerId = user.Id,
			CreatedByUserId = user.Id,
			ModifiedByUserId = user.Id,
			NormalizedName = normalizedName,
		};

		_ = await _repository.AddAsync(product);
		return CreatedAtAction(nameof(Get), new { id }, string.Empty);
	}

	private async Task<NoContentResult> UpdateExistingProductAsync(ProductCreationModel model, UserEntity user, Guid id)
	{
		var product = Mapper.Map<ProductEntity>(model) with
		{
			Id = id,
			OwnerId = user.Id, // todo only works for entities created by the user
			NormalizedName = model.Name!.ToUpperInvariant(),
			ModifiedByUserId = user.Id,
		};

		var x = await _repository.UpdateAsync(product);
		Debug.Assert(x > 0, "No rows were changed after update");
		return NoContent();
	}
}
