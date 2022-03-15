// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Authentication;

using IdentityModel.OidcClient;

using LoginResult = Gnomeshade.Interfaces.WebApi.Client.LoginResult;

namespace Gnomeshade.Interfaces.Avalonia.Core.Authentication;

/// <inheritdoc />
public sealed class AuthenticationService : IAuthenticationService
{
	private static readonly LoginRequest _loginRequest = new();

	private readonly BackgroundWorker _backgroundWorker = new();
	private readonly CancellationTokenSource _refreshCancellationSource = new();

	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly OidcClient _oidcClient;

	/// <summary>
	/// Initializes a new instance of the <see cref="AuthenticationService"/> class.
	/// </summary>
	/// <param name="gnomeshadeClient">API client for authenticating within the app.</param>
	/// <param name="oidcClient">An OIDC client for authenticating the user.</param>
	public AuthenticationService(IGnomeshadeClient gnomeshadeClient, OidcClient oidcClient)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_oidcClient = oidcClient;
	}

	/// <inheritdoc />
	public async Task SocialLogin()
	{
		var loginResult = await _oidcClient.LoginAsync(_loginRequest).ConfigureAwait(false);
		await _gnomeshadeClient.SocialRegister(loginResult.AccessToken).ConfigureAwait(false);

		_backgroundWorker.DoWork += async (_, _) =>
		{
			var refreshToken = loginResult.RefreshToken;
			var refreshDelay = (loginResult.AccessTokenExpiration - DateTimeOffset.Now) / 2;

			while (!_refreshCancellationSource.IsCancellationRequested)
			{
				await Task.Delay(refreshDelay).ConfigureAwait(false);
				var refreshResult = await _oidcClient.RefreshTokenAsync(refreshToken);
				await _gnomeshadeClient.SocialRegister(refreshResult.AccessToken).ConfigureAwait(false);

				refreshToken = refreshResult.RefreshToken;
				refreshDelay = (refreshResult.AccessTokenExpiration - DateTimeOffset.Now) / 2;
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
		await _gnomeshadeClient.LogOutAsync().ConfigureAwait(false);
	}
}
