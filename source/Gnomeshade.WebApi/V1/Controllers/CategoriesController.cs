// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using Gnomeshade.Data;
using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Products;

using Microsoft.AspNetCore.Mvc;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.WebApi.V1.Controllers;

/// <summary>CRUD operations on categories.</summary>
public sealed class CategoriesController : CreatableBase<CategoryRepository, CategoryEntity, Category, CategoryCreation>
{
	private readonly ProductRepository _productRepository;

	/// <summary>Initializes a new instance of the <see cref="CategoriesController"/> class.</summary>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	/// <param name="repository">The repository for performing CRUD operations on <see cref="CategoryEntity"/>.</param>
	/// <param name="dbConnection">Database connection for transaction management.</param>
	/// <param name="productRepository">The repository for performing CRUD operations on <see cref="ProductEntity"/>.</param>
	public CategoriesController(
		Mapper mapper,
		CategoryRepository repository,
		DbConnection dbConnection,
		ProductRepository productRepository)
		: base(mapper, repository, dbConnection)
	{
		_productRepository = productRepository;
	}

	/// <inheritdoc cref="IProductClient.GetCategoriesAsync"/>
	/// <response code="200">Successfully got the categories.</response>
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
		await using var dbTransaction = await DbConnection.OpenAndBeginTransaction();

		var linkedProductId = await GetLinkedProductId(id, creation, user, dbTransaction);
		var category = Mapper.Map<CategoryEntity>(creation) with
		{
			Id = id,
			ModifiedByUserId = user.Id,
			LinkedProductId = linkedProductId,
		};

		await Repository.UpdateAsync(category);
		await LinkProductToCategory(linkedProductId, id, creation.Name, user, dbTransaction);

		await dbTransaction.CommitAsync();

		return NoContent();
	}

	/// <inheritdoc />
	protected override async Task<ActionResult> CreateNewAsync(Guid id, CategoryCreation creation, UserEntity user)
	{
		await using var dbTransaction = await DbConnection.OpenAndBeginTransaction();

		var conflictingCategory = await Repository.FindByNameAsync(creation.Name, user.Id, dbTransaction);
		if (conflictingCategory is not null)
		{
			return Problem(
				$"{nameof(Category)} with the specified name already exists",
				Url.Action(nameof(Get), new { conflictingCategory.Id }),
				Status409Conflict);
		}

		var linkedProductId = await GetLinkedProductId(id, creation, user, dbTransaction);
		var category = Mapper.Map<CategoryEntity>(creation) with
		{
			Id = id,
			CreatedByUserId = user.Id,
			ModifiedByUserId = user.Id,
			LinkedProductId = linkedProductId,
		};

		_ = await Repository.AddAsync(category, dbTransaction);
		await LinkProductToCategory(linkedProductId, id, creation.Name, user, dbTransaction);

		await dbTransaction.CommitAsync();

		return CreatedAtAction(nameof(Get), new { id }, id);
	}

	private async Task<Guid?> GetLinkedProductId(
		Guid id,
		CategoryCreation creation,
		UserEntity user,
		DbTransaction dbTransaction)
	{
		if (!creation.LinkProduct)
		{
			return null;
		}

		var existingCategory = await Repository.FindByIdAsync(id, user.Id, dbTransaction);
		if (existingCategory?.LinkedProductId is not null)
		{
			return existingCategory.LinkedProductId;
		}

		var existingProduct = await _productRepository.FindByNameAsync(creation.Name, user.Id, dbTransaction);
		if (existingProduct is not null)
		{
			return existingProduct.Id;
		}

		var product = new ProductEntity
		{
			Id = id,
			OwnerId = creation.OwnerId ?? user.Id,
			CreatedByUserId = user.Id,
			ModifiedByUserId = user.Id,
			Name = creation.Name,
		};

		_ = await _productRepository.AddAsync(product, dbTransaction);

		return id;
	}

	private async Task LinkProductToCategory(
		Guid? linkedProductId,
		Guid id,
		string name,
		UserEntity user,
		DbTransaction dbTransaction)
	{
		if (linkedProductId is null)
		{
			return;
		}

		var linkedProduct = await _productRepository.GetByIdAsync(linkedProductId.Value, user.Id, dbTransaction);
		linkedProduct.CategoryId = id;
		linkedProduct.Name = name;
		await _productRepository.UpdateAsync(linkedProduct, dbTransaction);
	}
}
