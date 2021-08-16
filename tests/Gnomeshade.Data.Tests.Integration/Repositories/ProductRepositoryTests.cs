// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data;
using System.Threading.Tasks;

using FluentAssertions;
using FluentAssertions.Execution;

using Gnomeshade.Data.Repositories;
using Gnomeshade.TestingHelpers.Data.Fakers;

using NUnit.Framework;

using static Gnomeshade.Data.Tests.Integration.DatabaseInitialization;

namespace Gnomeshade.Data.Tests.Integration.Repositories
{
	public class ProductRepositoryTests : IDisposable
	{
		private IDbConnection _dbConnection = null!;
		private ProductRepository _repository = null!;

		[SetUp]
		public async Task SetUpAsync()
		{
			_dbConnection = await CreateConnectionAsync().ConfigureAwait(false);
			_repository = new(_dbConnection);
		}

		[TearDown]
		public void Dispose()
		{
			_dbConnection.Dispose();
			_repository.Dispose();
		}

		[Test]
		public async Task AddGetDelete_WithoutTransaction()
		{
			var productToAdd = new ProductFaker(TestUser).Generate();

			var id = await _repository.AddAsync(productToAdd);
			var getProduct = await _repository.GetByIdAsync(id);
			var findProduct = await _repository.FindByIdAsync(getProduct.Id);
			var allProducts = await _repository.GetAllAsync();

			var expectedProduct = productToAdd with
			{
				Id = id,
				CreatedAt = getProduct.CreatedAt,
				ModifiedAt = getProduct.ModifiedAt,
			};

			getProduct.Should().BeEquivalentTo(expectedProduct);
			findProduct.Should().BeEquivalentTo(expectedProduct);
			allProducts.Should().ContainSingle().Which.Should().BeEquivalentTo(expectedProduct);

			var productToUpdate = getProduct with { Description = "Foo" };
			var updatedId = await _repository.UpdateAsync(productToUpdate);
			var updatedProduct = await _repository.GetByIdAsync(updatedId);

			using (new AssertionScope())
			{
				updatedProduct.Id.Should().Be(getProduct.Id);
				updatedProduct.CreatedAt.Should().Be(getProduct.CreatedAt);
				updatedProduct.ModifiedAt.Should().BeAfter(getProduct.ModifiedAt);
				updatedProduct.Name.Should().Be(getProduct.Name);
				updatedProduct.Description.Should().NotBe(getProduct.Description);
			}

			await _repository.DeleteAsync(id);

			var afterDelete = await _repository.FindByIdAsync(id);
			afterDelete.Should().BeNull();
		}

		[Test]
		public async Task AddGetDelete_WithTransaction()
		{
			var productToAdd = new ProductFaker(TestUser).Generate();

			using var dbTransaction = _dbConnection.BeginTransaction();
			var id = await _repository.AddAsync(productToAdd, dbTransaction);
			dbTransaction.Commit();

			var getProduct = await _repository.GetByIdAsync(id);
			var expectedProduct = productToAdd with
			{
				Id = id,
				CreatedAt = getProduct.CreatedAt,
				ModifiedAt = getProduct.ModifiedAt,
			};

			getProduct.Should().BeEquivalentTo(expectedProduct);
			await _repository.DeleteAsync(id);
		}
	}
}
