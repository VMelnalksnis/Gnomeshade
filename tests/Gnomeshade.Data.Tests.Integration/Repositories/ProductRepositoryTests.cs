﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Data.Common;
using System.Threading.Tasks;

using Gnomeshade.Data.Repositories;
using Gnomeshade.Data.Tests.Integration.Fakers;

using Microsoft.Extensions.Logging.Abstractions;

using static Gnomeshade.Data.Tests.Integration.DatabaseInitialization;

namespace Gnomeshade.Data.Tests.Integration.Repositories;

public sealed class ProductRepositoryTests
{
	private DbConnection _dbConnection = null!;
	private ProductRepository _repository = null!;

	[SetUp]
	public async Task SetUpAsync()
	{
		_dbConnection = await CreateConnectionAsync().ConfigureAwait(false);
		_repository = new(NullLogger<ProductRepository>.Instance, _dbConnection);
	}

	[Test]
	public async Task AddGetDelete_WithoutTransaction()
	{
		var productToAdd = new ProductFaker(TestUser).Generate();

		await using var firstTransaction = await _dbConnection.BeginTransactionAsync();
		var id = await _repository.AddAsync(productToAdd, firstTransaction);

		var getProduct = await _repository.GetByIdAsync(id, TestUser.Id, firstTransaction);
		var findProduct = await _repository.FindByIdAsync(getProduct.Id, TestUser.Id, firstTransaction);
		var allProducts = await _repository.GetAsync(TestUser.Id, firstTransaction);
		await firstTransaction.CommitAsync();

		var expectedProduct = productToAdd with
		{
			Id = id,
			CreatedAt = getProduct.CreatedAt,
			ModifiedAt = getProduct.ModifiedAt,
		};

		getProduct.Should().BeEquivalentTo(expectedProduct);
		findProduct.Should().BeEquivalentTo(expectedProduct);
		allProducts.Should().ContainSingle().Which.Should().BeEquivalentTo(expectedProduct);

		await using var secondTransaction = await _dbConnection.BeginTransactionAsync();
		var productToUpdate = getProduct with { Sku = "123", Description = "Foo" };
		(await _repository.UpdateAsync(productToUpdate, secondTransaction)).Should().Be(1);
		var updatedProduct = await _repository.GetByIdAsync(productToUpdate.Id, TestUser.Id, secondTransaction);

		using (new AssertionScope())
		{
			updatedProduct.Id.Should().Be(getProduct.Id);
			updatedProduct.CreatedAt.Should().Be(getProduct.CreatedAt);
			updatedProduct.ModifiedAt.Should().BeGreaterThan(getProduct.ModifiedAt);
			updatedProduct.Name.Should().Be(getProduct.Name);
			updatedProduct.Sku.Should().Be("123");
			updatedProduct.Description.Should().Be("Foo");
		}

		(await _repository.DeleteAsync(id, TestUser.Id, secondTransaction)).Should().Be(1);
		await secondTransaction.CommitAsync();

		var afterDelete = await _repository.FindByIdAsync(id, TestUser.Id);
		afterDelete.Should().BeNull();
	}

	[Test]
	public async Task AddGetDelete_WithTransaction()
	{
		var productToAdd = new ProductFaker(TestUser).Generate();

		await using var dbTransaction = await _dbConnection.BeginTransactionAsync();
		var id = await _repository.AddAsync(productToAdd, dbTransaction);

		var getProduct = await _repository.GetByIdAsync(id, TestUser.Id, dbTransaction);
		var expectedProduct = productToAdd with
		{
			Id = id,
			CreatedAt = getProduct.CreatedAt,
			ModifiedAt = getProduct.ModifiedAt,
		};

		getProduct.Should().BeEquivalentTo(expectedProduct);

		(await _repository.DeleteAsync(id, TestUser.Id, dbTransaction)).Should().Be(1);

		await dbTransaction.CommitAsync();
	}
}
