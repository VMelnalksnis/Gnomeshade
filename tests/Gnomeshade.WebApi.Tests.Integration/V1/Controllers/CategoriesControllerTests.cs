﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
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

	public CategoriesControllerTests(WebserverFixture fixture)
		: base(fixture)
	{
	}

	[SetUp]
	public async Task SetUpAsync()
	{
		_client = await Fixture.CreateAuthorizedClientAsync();
	}

	[Test]
	public async Task Add_ShouldCreateExpected()
	{
		var categoryId = Guid.NewGuid();
		await _client.PutCategoryAsync(categoryId, new() { Name = $"{categoryId:N}" });

		var category = await _client.GetCategoryAsync(categoryId);
		(await _client.GetCategoriesAsync())
			.Should()
			.ContainEquivalentOf(category);
		category.CategoryId.Should().BeNull();

		var otherCategoryId = Guid.NewGuid();
		await _client.PutCategoryAsync(otherCategoryId, new() { Name = $"{otherCategoryId:N}" });

		await _client.PutCategoryAsync(categoryId, new() { Name = $"{Guid.NewGuid():N}", CategoryId = otherCategoryId });
		var updatedCategory = await _client.GetCategoryAsync(categoryId);
		updatedCategory.Name.Should().NotBeEquivalentTo(category.Name);
		updatedCategory.ModifiedAt.Should().BeGreaterThanOrEqualTo(category.ModifiedAt);
		updatedCategory.CategoryId.Should().Be(otherCategoryId);

		await _client.DeleteCategoryAsync(categoryId);

		(await FluentActions
				.Awaiting(() => _client.GetCategoryAsync(categoryId))
				.Should()
				.ThrowExactlyAsync<HttpRequestException>())
			.Which.StatusCode.Should()
			.Be(HttpStatusCode.NotFound);
	}
}