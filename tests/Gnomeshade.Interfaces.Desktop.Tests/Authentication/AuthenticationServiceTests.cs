// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.Desktop.Authentication;
using Gnomeshade.Interfaces.WebApi.Client;

using Moq;

using NUnit.Framework;

using VMelnalksnis.OAuth2;
using VMelnalksnis.OAuth2.Responses;

namespace Gnomeshade.Interfaces.Desktop.Tests.Authentication;

public class AuthenticationServiceTests
{
	[Test]
	public async Task SocialLogin_ShouldRefreshToken()
	{
		var verificationUri = new Uri("https://localhost/");
		var deviceResponse = new DeviceAuthorizationResponse(string.Empty, string.Empty, verificationUri, verificationUri, 2, 1);
		var oauth2Client = new Mock<IOAuth2Client>();
		oauth2Client
			.Setup(oauth2 => oauth2.StartDeviceFlowAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(deviceResponse);

		var tokenResponse = new TokenResponse(string.Empty, string.Empty, string.Empty, string.Empty, 2, string.Empty);
		oauth2Client
			.Setup(oauth2 => oauth2.GetDeviceFlowResultAsync(deviceResponse, It.IsAny<CancellationToken>()))
			.ReturnsAsync(tokenResponse);

		oauth2Client
			.Setup(oauth2 => oauth2.RefreshTokenAsync(tokenResponse, It.IsAny<CancellationToken>()))
			.ReturnsAsync(tokenResponse)
			.Verifiable();

		var gnomeshadeClient = new Mock<IGnomeshadeClient>();
		gnomeshadeClient
			.Setup(gnomeshade => gnomeshade.SocialRegister(It.IsAny<string>()))
			.Returns(Task.CompletedTask);

		var authenticationService = new AuthenticationService(gnomeshadeClient.Object, oauth2Client.Object);
		await authenticationService.SocialLogin();

		await Task.Delay(TimeSpan.FromSeconds(tokenResponse.ExpiresIn));

		oauth2Client.Verify(
			oauth2 => oauth2.RefreshTokenAsync(tokenResponse, It.IsAny<CancellationToken>()),
			Times.AtLeast(1));
	}
}
