// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using FluentAssertions;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.V1_0.Categories;

using NUnit.Framework;

namespace Gnomeshade.Interfaces.WebApi.Tests.Integration.V1_0.Categories;

[TestOf(typeof(CategoriesController))]
public class CategoriesControllerTests
{
	private ICategoryClient _client = null!;

	[SetUp]
	public async Task SetUpAsync()
	{
		_client = await WebserverSetup.CreateAuthorizedClientAsync();
	}

	[Test]
	public async Task Add_ShouldCreateExpected()
	{
		var tagId = Guid.NewGuid();
		await _client.PutCategoryAsync(tagId, new() { Name = $"{tagId:N}" });

		var tag = await _client.GetCategoryAsync(tagId);
		(await _client.GetCategoriesAsync())
			.Should()
			.ContainEquivalentOf(tag);

		var otherTagId = Guid.NewGuid();
		await _client.PutCategoryAsync(otherTagId, new() { Name = $"{otherTagId:N}" });

		await _client.PutCategoryAsync(tagId, new() { Name = $"{Guid.NewGuid():N}" });
		var updatedTag = await _client.GetCategoryAsync(tagId);
		updatedTag.Name.Should().NotBeEquivalentTo(tag.Name);
		updatedTag.ModifiedAt.Should().BeGreaterThan(tag.ModifiedAt);

		await _client.DeleteCategoryAsync(tagId);

		(await FluentActions
				.Awaiting(() => _client.GetCategoryAsync(tagId))
				.Should()
				.ThrowExactlyAsync<HttpRequestException>())
			.Which.StatusCode.Should()
			.Be(HttpStatusCode.NotFound);
	}
}
