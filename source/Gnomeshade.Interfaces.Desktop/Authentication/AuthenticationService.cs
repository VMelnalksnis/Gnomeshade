// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Authentication;

using VMelnalksnis.OAuth2;
using VMelnalksnis.OAuth2.Responses;

namespace Gnomeshade.Interfaces.Desktop.Authentication;

/// <inheritdoc />
public sealed class AuthenticationService : IAuthenticationService
{
	private readonly BackgroundWorker _backgroundWorker = new();
	private readonly CancellationTokenSource _refreshCancellationSource = new();

	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly IOAuth2Client _oAuth2Client;

	/// <summary>
	/// Initializes a new instance of the <see cref="AuthenticationService"/> class.
	/// </summary>
	/// <param name="gnomeshadeClient">API client for authenticating within the app.</param>
	/// <param name="oAuth2Client">API client for authenticating using social login.</param>
	public AuthenticationService(IGnomeshadeClient gnomeshadeClient, IOAuth2Client oAuth2Client)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_oAuth2Client = oAuth2Client;
	}

	/// <inheritdoc />
	public async Task SocialLogin()
	{
		var deviceAuthorizationResponse = await _oAuth2Client.StartDeviceFlowAsync().ConfigureAwait(false);
		var processInfo = deviceAuthorizationResponse.GetProcessStartInfoForUserApproval();
		Process.Start(processInfo);
		var tokenResponse = await _oAuth2Client.GetDeviceFlowResultAsync(deviceAuthorizationResponse).ConfigureAwait(false);
		await _gnomeshadeClient.SocialRegister(tokenResponse.AccessToken).ConfigureAwait(false);

		_backgroundWorker.DoWork += async (_, _) =>
		{
			while (!_refreshCancellationSource.IsCancellationRequested)
			{
				await Task.Delay(TimeSpan.FromSeconds((double)tokenResponse.ExpiresIn / 2)).ConfigureAwait(false);
				tokenResponse = await _oAuth2Client.RefreshTokenAsync(tokenResponse, _refreshCancellationSource.Token);
				await _gnomeshadeClient.SocialRegister(tokenResponse.AccessToken).ConfigureAwait(false);
			}
		};
		_backgroundWorker.RunWorkerAsync();
	}

	/// <inheritdoc />
	public async Task<LoginResult> Login(Login login)
	{
		return await _gnomeshadeClient.LogInAsync(login).ConfigureAwait(false);
	}

	/// <inheritdoc />
	public async Task Logout()
	{
		_refreshCancellationSource.Cancel();
		await _gnomeshadeClient.LogOutAsync().ConfigureAwait(false);
	}
}
