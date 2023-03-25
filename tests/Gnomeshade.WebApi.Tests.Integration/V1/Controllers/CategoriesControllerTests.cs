// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Tests.Integration.Fixtures;
using Gnomeshade.WebApi.V1.Controllers;

namespace Gnomeshade.WebApi.Tests.Integration.V1.Controllers;

[TestOf(typeof(CategoriesController))]
public sealed class CategoriesControllerTests : WebserverTests
{
	private IProductClient _client = null!;
	private Guid _categoryId;

	public CategoriesControllerTests(WebserverFixture fixture)
		: base(fixture)
	{
	}

	[SetUp]
	public async Task SetUpAsync()
	{
		_client = await Fixture.CreateAuthorizedClientAsync();
		_categoryId = Guid.NewGuid();
	}

	[Test]
	public async Task Add_ShouldCreateExpected()
	{
		await _client.PutCategoryAsync(_categoryId, new() { Name = $"{_categoryId:N}" });

		var category = await _client.GetCategoryAsync(_categoryId);
		(await _client.GetCategoriesAsync())
			.Should()
			.ContainEquivalentOf(category);
		category.CategoryId.Should().BeNull();

		var otherCategoryId = Guid.NewGuid();
		await _client.PutCategoryAsync(otherCategoryId, new() { Name = $"{otherCategoryId:N}" });

		await _client.PutCategoryAsync(_categoryId, new() { Name = $"{Guid.NewGuid():N}", CategoryId = otherCategoryId });
		var updatedCategory = await _client.GetCategoryAsync(_categoryId);
		updatedCategory.Name.Should().NotBeEquivalentTo(category.Name);
		updatedCategory.ModifiedAt.Should().BeGreaterThanOrEqualTo(category.ModifiedAt);
		updatedCategory.CategoryId.Should().Be(otherCategoryId);

		await _client.DeleteCategoryAsync(otherCategoryId);

		(await FluentActions
				.Awaiting(() => _client.GetCategoryAsync(otherCategoryId))
				.Should()
				.ThrowExactlyAsync<HttpRequestException>())
			.Which.StatusCode.Should()
			.Be(HttpStatusCode.NotFound);
	}

	[Test]
	public async Task Add_WithLinkedProduct_ShouldCreateExpected()
	{
		await _client.PutCategoryAsync(_categoryId, new() { Name = $"{_categoryId:N}", LinkProduct = true });

		var category = await _client.GetCategoryAsync(_categoryId);
		var linkedProduct = await _client.GetProductAsync(_categoryId);
		linkedProduct.Name.Should().Be(category.Name);

		var newName = $"{Guid.NewGuid():N}";
		await _client.PutCategoryAsync(_categoryId, new() { Name = newName, LinkProduct = true });
		var updatedCategory = await _client.GetCategoryAsync(_categoryId);
		linkedProduct = await _client.GetProductAsync(_categoryId);
		updatedCategory.Name.Should().Be(newName);
		linkedProduct.Name.Should().Be(newName);

		await _client.PutCategoryAsync(_categoryId, new() { Name = newName, LinkProduct = false });
		updatedCategory = await _client.GetCategoryAsync(_categoryId);
		updatedCategory.LinkedProductId.Should().BeNull();
	}

	[TearDown]
	public async Task TearDownAsync()
	{
		var categories = await _client.GetCategoriesAsync();
		var category = categories.SingleOrDefault(category => category.Id == _categoryId);
		if (category is not null)
		{
			await _client.DeleteCategoryAsync(category.Id);
		}
	}
}
