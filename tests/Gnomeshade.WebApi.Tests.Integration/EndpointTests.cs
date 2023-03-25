// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Net;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Tests.Integration.Fixtures;

namespace Gnomeshade.WebApi.Tests.Integration;

public sealed class EndpointTests : WebserverTests
{
	public EndpointTests(WebserverFixture fixture)
		: base(fixture)
	{
	}

	[TestCaseSource(nameof(Endpoints))]
	public async Task ShouldReturnOk(Uri requestUri)
	{
		var apiClient = Fixture.CreateHttpClient();

		using var response = await apiClient.GetAsync(requestUri);
		response.StatusCode.Should().Be(HttpStatusCode.OK);
	}

	[TestCaseSource(nameof(Redirects))]
	public async Task ShouldRedirect(Uri requestUri, Uri redirectUri)
	{
		var apiClient = Fixture.CreateHttpClient();

		using var response = await apiClient.GetAsync(requestUri);
		response.StatusCode.Should().Be(HttpStatusCode.Found);

		var location = response.Headers.Location;
		location.Should().BeEquivalentTo(redirectUri);
	}

	private static IEnumerable Endpoints()
	{
		yield return new Uri("/", UriKind.Relative);
		yield return new Uri("/Identity/Account/Register", UriKind.Relative);
		yield return new Uri("/swagger/index.html", UriKind.Relative);
		yield return new Uri("/swagger/v1/swagger.json", UriKind.Relative);
	}

	private static IEnumerable Redirects()
	{
		yield return new TestCaseData(
			new Uri("/Identity/Account/ExternalLogin", UriKind.Relative),
			new Uri("/Identity/Account/Login", UriKind.Relative));

		yield return new TestCaseData(
			new Uri("/Identity/Account/LoginWith2Fa", UriKind.Relative),
			new Uri("/Identity/Account/Login", UriKind.Relative));

		yield return new TestCaseData(
			new Uri("/Identity/Account/LoginWithRecoveryCode", UriKind.Relative),
			new Uri("/Identity/Account/Login", UriKind.Relative));
	}
}
