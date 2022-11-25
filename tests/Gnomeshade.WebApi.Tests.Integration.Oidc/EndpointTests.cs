// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Gnomeshade.WebApi.Tests.Integration.Oidc;

[TestFixtureSource(typeof(OidcFixtureSource))]
public sealed class EndpointTests
{
	private readonly KeycloakFixture _fixture;

	private HttpClient _client = null!;

	public EndpointTests(KeycloakFixture fixture)
	{
		_fixture = fixture;
	}

	[SetUp]
	public void SetUp()
	{
		_client = _fixture.CreateHttpClient();
	}

	[TestCaseSource(nameof(Endpoints))]
	public async Task ShouldReturnOk(Uri requestUri)
	{
		using var response = await _client.GetAsync(requestUri);
		response.StatusCode.Should().Be(HttpStatusCode.OK);
	}

	[TestCaseSource(nameof(Redirects))]
	public async Task ShouldRedirect(Uri requestUri, Uri redirectUri)
	{
		using var response = await _client.GetAsync(requestUri);
		response.StatusCode.Should().Be(HttpStatusCode.Found);

		var location = response.Headers.Location;
		location.Should().BeEquivalentTo(redirectUri);
	}

	[Test]
	public async Task Health_ShouldReturnHealthy()
	{
		using var response = await _client.GetAsync("/health");
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var content = await response.Content.ReadAsStringAsync();
		content.Should().Be(nameof(HealthStatus.Healthy));
	}

	private static IEnumerable Endpoints()
	{
		yield return new Uri("/", UriKind.Relative);
		yield return new Uri("/Identity/Account/Register", UriKind.Relative);
		yield return new Uri("/Identity/Account/Login", UriKind.Relative);
	}

	private static IEnumerable Redirects()
	{
		yield return new TestCaseData(
			new Uri("/Identity/Account/ExternalLogin", UriKind.Relative),
			new Uri("/Identity/Account/Login", UriKind.Relative));
	}
}
