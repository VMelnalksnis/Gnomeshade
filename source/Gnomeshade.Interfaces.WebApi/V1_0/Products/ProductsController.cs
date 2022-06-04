﻿// Copyright 2021 Valters Melnalksnis
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
using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Products;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;
using Gnomeshade.Interfaces.WebApi.OpenApi;
using Gnomeshade.Interfaces.WebApi.V1_0.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.Interfaces.WebApi.V1_0.Products;

/// <summary>CRUD operations on account entity.</summary>
public sealed class ProductsController : CreatableBase<ProductRepository, ProductEntity, Product, ProductCreation>
{
	private readonly PurchaseRepository _purchaseRepository;

	/// <summary>Initializes a new instance of the <see cref="ProductsController"/> class.</summary>
	/// <param name="applicationUserContext">Context for getting the current application user.</param>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="repository">The repository for performing CRUD operations on <see cref="ProductEntity"/>.</param>
	/// <param name="purchaseRepository">The repository for performing CRUD operations on <see cref="PurchaseEntity"/>.</param>
	public ProductsController(
		ApplicationUserContext applicationUserContext,
		Mapper mapper,
		ILogger<ProductsController> logger,
		ProductRepository repository,
		PurchaseRepository purchaseRepository)
		: base(applicationUserContext, mapper, logger, repository)
	{
		_purchaseRepository = purchaseRepository;
	}

	/// <inheritdoc cref="IProductClient.GetProductAsync"/>
	/// <response code="200">Successfully got the product.</response>
	/// <response code="404">Product with the specified id does not exist.</response>
	[ProducesResponseType(typeof(Product), Status200OK)]
	public override Task<ActionResult<Product>> Get(Guid id, CancellationToken cancellationToken) =>
		base.Get(id, cancellationToken);

	/// <inheritdoc cref="IProductClient.GetProductsAsync"/>
	/// <response code="200">Successfully got all products.</response>
	[HttpGet]
	[ProducesResponseType(typeof(List<Product>), Status200OK)]
	public override Task<List<Product>> Get(CancellationToken cancellationToken) =>
		base.Get(cancellationToken);

	/// <inheritdoc cref="IProductClient.PutProductAsync"/>
	/// <response code="201">A new product was created.</response>
	/// <response code="204">An existing product was replaced.</response>
	/// <response code="409">A product with the specified name already exists.</response>
	public override Task<ActionResult> Put(Guid id, [FromBody] ProductCreation product) =>
		base.Put(id, product);

	/// <inheritdoc cref="IProductClient.GetProductPurchasesAsync"/>
	/// <response code="200">Successfully got the purchases.</response>
	/// <response code="404">Product with the specified id does not exist.</response>
	[HttpGet("{id:guid}/Purchases")]
	[ProducesResponseType(typeof(List<Purchase>), Status200OK)]
	[ProducesStatus404NotFound]
	public async Task<ActionResult<List<Purchase>>> Purchases(Guid id, CancellationToken cancellationToken)
	{
		var product = await Repository.FindByIdAsync(id, ApplicationUser.Id, cancellationToken);
		if (product is null)
		{
			return NotFound();
		}

		var purchases = await _purchaseRepository.GetAllForProduct(id, ApplicationUser.Id, cancellationToken);
		var models = purchases.Select(purchase => Mapper.Map<Purchase>(purchase)).ToList();
		return Ok(models);
	}

	/// <inheritdoc />
	protected override async Task<ActionResult> UpdateExistingAsync(
		Guid id,
		ProductCreation creation,
		UserEntity user)
	{
		var product = Mapper.Map<ProductEntity>(creation) with
		{
			Id = id,
			OwnerId = user.Id, // todo only works for entities created by the user
			NormalizedName = creation.Name!.ToUpperInvariant(),
			ModifiedByUserId = user.Id,
		};

		_ = await Repository.UpdateAsync(product);
		return NoContent();
	}

	/// <inheritdoc />
	protected override async Task<ActionResult> CreateNewAsync(Guid id, ProductCreation creation, UserEntity user)
	{
		var normalizedName = creation.Name!.ToUpperInvariant();
		var conflictingProduct = await Repository.FindByNameAsync(normalizedName, user.Id);
		if (conflictingProduct is not null)
		{
			return Problem(
				"Product with the specified name already exists",
				Url.Action(nameof(Get), new { conflictingProduct.Id }),
				Status409Conflict);
		}

		var product = Mapper.Map<ProductEntity>(creation) with
		{
			Id = id,
			OwnerId = user.Id,
			CreatedByUserId = user.Id,
			ModifiedByUserId = user.Id,
			NormalizedName = normalizedName,
		};

		_ = await Repository.AddAsync(product);
		return CreatedAtAction(nameof(Get), new { id }, id);
	}
}
