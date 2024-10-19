// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Tests.Integration.Oidc.Fixtures;

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Gnomeshade.WebApi.Tests.Integration.Oidc;

[FixtureDataSource]
public sealed class EndpointTests(KeycloakFixture fixture) : WebApplicationTests(fixture)
{
	public static IEnumerable<Uri> Endpoints()
	{
		yield return new("/", UriKind.Relative);
		yield return new("/Identity/Account/Register", UriKind.Relative);
		yield return new("/Identity/Account/Login", UriKind.Relative);
	}

	public static IEnumerable<(Uri RequestUri, Uri RedirectUri)> Redirects()
	{
		yield return (
			new("/Identity/Account/ExternalLogin", UriKind.Relative),
			new("/Identity/Account/Login", UriKind.Relative));
	}

	[Test]
	[MethodDataSource(nameof(Endpoints))]
	public async Task ShouldReturnOk(Uri requestUri)
	{
		using var client = Fixture.CreateHttpClient();
		using var response = await client.GetAsync(requestUri);
		response.StatusCode.Should().Be(HttpStatusCode.OK);
	}

	[Test]
	[MethodDataSource(nameof(Redirects))]
	public async Task ShouldRedirect(Uri requestUri, Uri redirectUri)
	{
		using var client = Fixture.CreateHttpClient();
		using var response = await client.GetAsync(requestUri);
		response.StatusCode.Should().Be(HttpStatusCode.Found);

		var location = response.Headers.Location;
		location.Should().BeEquivalentTo(redirectUri);
	}

	[Test]
	public async Task Health_ShouldReturnHealthy()
	{
		using var client = Fixture.CreateHttpClient();
		using var response = await client.GetAsync("/api/v1.0/Health");
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var content = await response.Content.ReadAsStringAsync();
		content.Should().Be(nameof(HealthStatus.Healthy));
	}
}
