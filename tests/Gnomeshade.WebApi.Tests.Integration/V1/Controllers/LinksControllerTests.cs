// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Models;
using Gnomeshade.WebApi.Tests.Integration.Fixtures;
using Gnomeshade.WebApi.V1.Controllers;

namespace Gnomeshade.WebApi.Tests.Integration.V1.Controllers;

[TestOf(typeof(LinksController))]
public sealed class LinksControllerTests : WebserverTests
{
	public LinksControllerTests(WebserverFixture fixture)
		: base(fixture)
	{
	}

	[Test]
	public async Task AddGetDelete()
	{
		var client = await Fixture.CreateAuthorizedClientAsync();

		var linkCreation = new LinkCreation { Uri = new("https://localhost/documents/1") };
		var id = Guid.NewGuid();
		await client.PutLinkAsync(id, linkCreation);

		var link = await client.GetLinkAsync(id);
		var links = await client.GetLinksAsync();

		link.Id.Should().Be(id);
		link.Uri.Should().Be(linkCreation.Uri.ToString());
		links.Should().ContainEquivalentOf(link);

		await client.DeleteLinkAsync(id);

		(await FluentActions
				.Awaiting(() => client.GetLinkAsync(id))
				.Should()
				.ThrowExactlyAsync<HttpRequestException>())
			.Which.StatusCode.Should()
			.Be(HttpStatusCode.NotFound);
	}

	[Test]
	public async Task UniqueConstraint()
	{
		var client = await Fixture.CreateAuthorizedClientAsync();

		var id = Guid.NewGuid();
		var linkCreation = new LinkCreation { Uri = new($"https://localhost/documents/{id:N}") };
		await client.PutLinkAsync(id, linkCreation);

		(await FluentActions
				.Awaiting(() => client.PutLinkAsync(Guid.NewGuid(), linkCreation))
				.Should()
				.ThrowExactlyAsync<HttpRequestException>())
			.Which.StatusCode.Should()
			.Be(HttpStatusCode.Conflict, $"{nameof(Link.Uri)} is unique");

		await client.DeleteLinkAsync(id);

		await FluentActions
			.Awaiting(() => client.PutLinkAsync(Guid.NewGuid(), linkCreation))
			.Should()
			.NotThrowAsync("unique constraint should ignore deleted links");
	}
}
