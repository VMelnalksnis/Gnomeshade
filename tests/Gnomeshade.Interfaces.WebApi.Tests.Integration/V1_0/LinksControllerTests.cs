﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using FluentAssertions;

using Gnomeshade.Interfaces.WebApi.Models;
using Gnomeshade.Interfaces.WebApi.V1_0;

using NUnit.Framework;

namespace Gnomeshade.Interfaces.WebApi.Tests.Integration.V1_0;

[TestOf(typeof(LinksController))]
public class LinksControllerTests
{
	[Test]
	public async Task AddGetDelete()
	{
		var client = await WebserverSetup.CreateAuthorizedClientAsync();

		var linkCreation = new LinkCreation
		{
			Uri = new("https://localhost/documents/1"),
		};

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
}