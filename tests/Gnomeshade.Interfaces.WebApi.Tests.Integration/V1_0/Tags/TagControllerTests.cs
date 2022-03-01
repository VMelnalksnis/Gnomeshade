// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using FluentAssertions;

using Gnomeshade.Interfaces.WebApi.Client;

using NUnit.Framework;

namespace Gnomeshade.Interfaces.WebApi.Tests.Integration.V1_0.Tags;

public class TagControllerTests
{
	private ITagClient _client = null!;

	[SetUp]
	public async Task SetUpAsync()
	{
		_client = await WebserverSetup.CreateAuthorizedClientAsync();
	}

	[Test]
	public async Task Add_ShouldCreateExpected()
	{
		var tagId = Guid.NewGuid();
		await _client.PutTagAsync(tagId, new() { Name = $"{tagId:N}" });

		var tag = await _client.GetTagAsync(tagId);
		(await _client.GetTagsAsync())
			.Should()
			.ContainEquivalentOf(tag);

		var otherTagId = Guid.NewGuid();
		await _client.PutTagAsync(otherTagId, new() { Name = $"{otherTagId:N}" });

		await _client.TagTagAsync(tagId, otherTagId);
		(await _client.GetTagTagsAsync(tagId))
			.Should()
			.ContainSingle()
			.Which.Id.Should()
			.Be(otherTagId);

		await _client.UntagTagAsync(tagId, otherTagId);
		(await _client.GetTagTagsAsync(tagId))
			.Should()
			.BeEmpty();

		await _client.PutTagAsync(tagId, new() { Name = $"{Guid.NewGuid():N}" });
		var updatedTag = await _client.GetTagAsync(tagId);
		updatedTag.Name.Should().NotBeEquivalentTo(tag.Name);
		updatedTag.ModifiedAt.Should().BeAfter(tag.ModifiedAt);

		await _client.DeleteTagAsync(tagId);

		(await FluentActions
				.Awaiting(() => _client.GetTagAsync(tagId))
				.Should()
				.ThrowExactlyAsync<HttpRequestException>())
			.Which.StatusCode.Should()
			.Be(HttpStatusCode.NotFound);
	}
}
