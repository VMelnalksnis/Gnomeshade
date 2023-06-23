// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using Gnomeshade.TestingHelpers.Models;
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

		await _client.DeleteCategoryAsync(_categoryId);
		await _client.DeleteCategoryAsync(otherCategoryId);

		await ShouldThrowNotFound(() => _client.GetCategoryAsync(otherCategoryId));
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

	[Test]
	public async Task Delete_CategoryReference()
	{
		var category = await _client.CreateCategoryAsync();
		var id = category.Id;

		var linkedId = Guid.NewGuid();
		await _client.PutCategoryAsync(linkedId, new() { Name = $"{linkedId:N}", CategoryId = id });

		await ShouldThrowConflict(() => _client.DeleteCategoryAsync(id));
		(await _client.GetCategoryAsync(id)).Should().BeEquivalentTo(category);

		await _client.DeleteCategoryAsync(linkedId);
		await _client.DeleteCategoryAsync(id);

		await ShouldThrowNotFound(() => _client.GetCategoryAsync(id));
	}

	[Test]
	public async Task Delete_ProductReference()
	{
		var id = Guid.NewGuid();
		await _client.PutCategoryAsync(id, new() { Name = $"{id:N}", LinkProduct = true });
		var category = await _client.GetCategoryAsync(id);

		await ShouldThrowConflict(() => _client.DeleteCategoryAsync(id));
		(await _client.GetCategoryAsync(id)).Should().BeEquivalentTo(category);

		await _client.DeleteProductAsync(id);
		await _client.DeleteCategoryAsync(id);

		await ShouldThrowNotFound(() => _client.GetCategoryAsync(id));
	}
}
