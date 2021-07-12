// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data;
using System.Threading.Tasks;

using FluentAssertions;

using NUnit.Framework;

using Tracking.Finance.Data.Models;
using Tracking.Finance.Data.Repositories;

using static Tracking.Finance.Data.Tests.Integration.DatabaseInitialization;

namespace Tracking.Finance.Data.Tests.Integration.Repositories
{
	public class ProductRepositoryTests : IDisposable
	{
		private IDbConnection _dbConnection = null!;
		private ProductRepository _repository = null!;
		private Product _defaultProduct = null!;

		[SetUp]
		public async Task SetUpAsync()
		{
			_dbConnection = await CreateConnectionAsync().ConfigureAwait(false);
			_repository = new(_dbConnection);

			_defaultProduct = new()
			{
				OwnerId = TestUser.Id,
				CreatedByUserId = TestUser.Id,
				ModifiedByUserId = TestUser.Id,
				Name = "Foo",
				NormalizedName = "Foo".ToUpperInvariant(),
				Description = "Some description",
			};
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
			var productToAdd = _defaultProduct with { };

			var id = await _repository.AddAsync(productToAdd);
			var getProduct = await _repository.GetByIdAsync(id);
			var findProduct = await _repository.FindByIdAsync(id);
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

			await _repository.DeleteAsync(id);

			var afterDelete = await _repository.FindByIdAsync(id);
			afterDelete.Should().BeNull();
		}

		[Test]
		public async Task AddGetDelete_WithTransaction()
		{
			var productToAdd = _defaultProduct with { };

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
