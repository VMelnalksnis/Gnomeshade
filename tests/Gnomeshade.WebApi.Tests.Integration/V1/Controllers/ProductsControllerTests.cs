// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Products;
using Gnomeshade.WebApi.Tests.Integration.Fixtures;
using Gnomeshade.WebApi.V1.Controllers;

namespace Gnomeshade.WebApi.Tests.Integration.V1.Controllers;

[TestOf(typeof(ProductsController))]
public sealed class ProductsControllerTests : WebserverTests
{
	private IGnomeshadeClient _client = null!;

	public ProductsControllerTests(WebserverFixture fixture)
		: base(fixture)
	{
	}

	[SetUp]
	public async Task SetUpAsync()
	{
		_client = await Fixture.CreateAuthorizedClientAsync();
	}

	[Test]
	public async Task Put_ShouldReturnConflictOnDuplicateName()
	{
		var creationModel = CreateUniqueProduct() with { Description = "Foo" };
		await _client.PutProductAsync(Guid.NewGuid(), creationModel);

		var exception =
			await FluentActions
				.Awaiting(() => _client.PutProductAsync(Guid.NewGuid(), creationModel))
				.Should()
				.ThrowExactlyAsync<HttpRequestException>();

		exception.Which.StatusCode.Should().Be(HttpStatusCode.Conflict);
	}

	[Test]
	public async Task Put()
	{
		var unitId = Guid.NewGuid();
		var unit = new UnitCreation { Name = $"{unitId:N}" };
		await _client.PutUnitAsync(unitId, unit);

		var categoryId = Guid.NewGuid();
		var category = new CategoryCreation { Name = $"{categoryId:N}" };
		await _client.PutCategoryAsync(categoryId, category);

		var creationModel = CreateUniqueProduct() with
		{
			Description = "Foo",
			UnitId = unitId,
			CategoryId = categoryId,
		};
		var productId = Guid.NewGuid();
		var product = await PutAndGet(productId, creationModel);

		using (new AssertionScope())
		{
			product.Name.Should().Be(creationModel.Name);
			product.Description.Should().Be(creationModel.Description);
			product.UnitId.Should().Be(unitId);
			product.CategoryId.Should().Be(categoryId);
		}

		var productWithoutChanges = await PutAndGet(productId, creationModel);

		productWithoutChanges.Should().BeEquivalentTo(product, WithoutModifiedAt);
		productWithoutChanges.ModifiedAt.Should().BeGreaterThanOrEqualTo(product.ModifiedAt);

		var changedCreationModel = creationModel with { Description = null };
		var productWithChanges = await PutAndGet(productId, changedCreationModel);

		productWithChanges.Should().BeEquivalentTo(product, WithoutModifiedAtAndDescription);
		productWithChanges.Description.Should().BeNull();

		var anotherCreationModel = CreateUniqueProduct();
		_ = await PutAndGet(productId, anotherCreationModel);
	}

	[Test]
	public async Task Purchases_ShouldReturnNotFound()
	{
		(await FluentActions
				.Awaiting(() => _client.GetProductPurchasesAsync(Guid.NewGuid()))
				.Should()
				.ThrowExactlyAsync<HttpRequestException>())
			.Which.StatusCode.Should()
			.Be(HttpStatusCode.NotFound);
	}

	private static ProductCreation CreateUniqueProduct()
	{
		return new() { Name = Guid.NewGuid().ToString("N") };
	}

	private static EquivalencyAssertionOptions<Product> WithoutModifiedAt(
		EquivalencyAssertionOptions<Product> options)
	{
		return options.ComparingByMembers<Product>().Excluding(model => model.ModifiedAt);
	}

	private static EquivalencyAssertionOptions<Product> WithoutModifiedAtAndDescription(
		EquivalencyAssertionOptions<Product> options)
	{
		return WithoutModifiedAt(options).Excluding(model => model.Description);
	}

	private async Task<Product> PutAndGet(Guid id, ProductCreation creation)
	{
		await _client.PutProductAsync(id, creation);
		return await _client.GetProductAsync(id);
	}
}
