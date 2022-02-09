// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using FluentAssertions;

using NUnit.Framework;

using VMelnalksnis.OAuth2.Keycloak;
using VMelnalksnis.OAuth2.Responses;

namespace VMelnalksnis.OAuth2.Tests.Integration.Keycloak;

public class KeycloakOAuth2ClientTests
{
	[Test]
	[Ignore("Requires user interaction via browser")]
	public async Task SignInRefreshRevoke()
	{
		var keycloakClient = TestConfiguration.GetRequiredService<KeycloakOAuth2Client>();

		var deviceAuthorizationResponse = await keycloakClient.StartDeviceFlowAsync();
		Process.Start(deviceAuthorizationResponse.GetProcessStartInfoForUserApproval());
		var tokenResponse = await keycloakClient.GetDeviceFlowResultAsync(deviceAuthorizationResponse);

		var refreshedTokenResponse = await keycloakClient.RefreshTokenAsync(tokenResponse);
		await keycloakClient.RevokeTokenAsync(refreshedTokenResponse.AccessToken);
		await keycloakClient.RevokeTokenAsync(refreshedTokenResponse.RefreshToken);

		(await FluentActions
			.Awaiting(() => keycloakClient.RefreshTokenAsync(refreshedTokenResponse))
			.Should()
			.ThrowAsync<HttpRequestException>())
			.Which.StatusCode.Should()
			.Be(HttpStatusCode.BadRequest);
	}
}
