// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;
using Gnomeshade.Interfaces.WebApi.Models.Products;
using Gnomeshade.Interfaces.WebApi.V1_0.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.Interfaces.WebApi.V1_0.Products;

/// <summary>
/// CRUD operations on account entity.
/// </summary>
public sealed class ProductController : FinanceControllerBase<ProductEntity, Product>
{
	private readonly ProductRepository _repository;

	public ProductController(
		ProductRepository repository,
		ApplicationUserContext applicationUserContext,
		Mapper mapper)
		: base(applicationUserContext, mapper)
	{
		_repository = repository;
	}

	[HttpGet("{id:guid}")]
	[ProducesResponseType(Status200OK)]
	[ProducesResponseType(typeof(ProblemDetails), Status404NotFound)]
	public async Task<ActionResult<Product>> Get(Guid id, CancellationToken cancellationToken)
	{
		return await Find(() => _repository.FindByIdAsync(id, ApplicationUser.Id, cancellationToken));
	}

	[HttpGet]
	[ProducesResponseType(Status200OK)]
	public async Task<ActionResult<List<Product>>> GetAll(CancellationToken cancellationToken)
	{
		var products = await _repository.GetAllAsync(ApplicationUser.Id, cancellationToken);
		var productModels = products.Select(account => MapToModel(account)).ToList();
		return Ok(productModels);
	}

	/// <summary>
	/// Creates a new product, or replaces and existing one if one exists with the specified id.
	/// </summary>
	/// <param name="model">The product to create or replace.</param>
	/// <returns>The id of the created or replaced product.</returns>
	/// <response code="200">An existing product was replaced.</response>
	/// <response code="201">A new product was created.</response>
	[HttpPut]
	[ProducesResponseType(Status200OK)]
	[ProducesResponseType(Status201Created)]
	public async Task<ActionResult<Guid>> Put([FromBody, BindRequired] ProductCreationModel model)
	{
		var normalizedName = model.Name!.ToUpperInvariant();
		var existingByName = await _repository.FindByNameAsync(normalizedName, ApplicationUser.Id);

		if ((model.Id is not null || (model.Id is null && existingByName is not null)) &&
			existingByName is not null &&
			existingByName.Id != model.Id)
		{
			// todo use standard error model
			return BadRequest($"Product with name {model.Name} already exists");
		}

		var existingById = model.Id is not null
			? await _repository.FindByIdAsync(model.Id.Value, ApplicationUser.Id)
			: default;

		return existingById is null
			? await CreateNewProductAsync(model, ApplicationUser)
			: await UpdateExistingProductAsync(model, ApplicationUser);
	}

	private async Task<ActionResult<Guid>> CreateNewProductAsync(ProductCreationModel creationModel, UserEntity user)
	{
		var product = Mapper.Map<ProductEntity>(creationModel) with
		{
			OwnerId = user.Id,
			CreatedByUserId = user.Id,
			ModifiedByUserId = user.Id,
			NormalizedName = creationModel.Name!.ToUpperInvariant(),
		};

		var id = await _repository.AddAsync(product);
		return CreatedAtAction(nameof(Get), new { id }, id);
	}

	private async Task<ActionResult<Guid>> UpdateExistingProductAsync(ProductCreationModel creationModel, UserEntity user)
	{
		var product = Mapper.Map<ProductEntity>(creationModel);
		product.NormalizedName = product.Name.ToUpperInvariant();
		product.ModifiedByUserId = user.Id;

		_ = await _repository.UpdateAsync(product);
		return Ok(creationModel.Id);
	}
}
