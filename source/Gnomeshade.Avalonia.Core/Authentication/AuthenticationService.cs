// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Client.Results;
using Gnomeshade.WebApi.Models.Authentication;

using IdentityModel.OidcClient;

using LoginResult = Gnomeshade.WebApi.Client.LoginResult;

namespace Gnomeshade.Avalonia.Core.Authentication;

/// <inheritdoc />
public sealed class AuthenticationService : IAuthenticationService
{
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly GnomeshadeTokenCache _gnomeshadeTokenCache;
	private readonly ICredentialStorageService _credentialStorageService;
	private readonly OidcClient _oidcClient;

	/// <summary>Initializes a new instance of the <see cref="AuthenticationService"/> class.</summary>
	/// <param name="gnomeshadeClient">API client for authenticating within the app.</param>
	/// <param name="gnomeshadeTokenCache">Gnomeshade API token cache for preserving tokens between requests.</param>
	/// <param name="credentialStorageService">Service for persisting credentials between application runs.</param>
	/// <param name="oidcClient">OIDC client for token management.</param>
	public AuthenticationService(
		IGnomeshadeClient gnomeshadeClient,
		GnomeshadeTokenCache gnomeshadeTokenCache,
		ICredentialStorageService credentialStorageService,
		OidcClient oidcClient)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_gnomeshadeTokenCache = gnomeshadeTokenCache;
		_credentialStorageService = credentialStorageService;
		_oidcClient = oidcClient;
	}

	/// <inheritdoc />
	public async Task<ExternalLoginResult> SocialLogin()
	{
		if (_credentialStorageService.TryGetRefreshToken(out var existingRefreshToken))
		{
			_gnomeshadeTokenCache.SetRefreshToken(existingRefreshToken);
		}

		var result = await _gnomeshadeClient.SocialRegister();

		if (result is LoggedIn && _gnomeshadeTokenCache.Refresh is { } refreshToken)
		{
			_credentialStorageService.StoreRefreshToken(refreshToken);
			_gnomeshadeTokenCache.RefreshTokenChanged += (_, args) => _credentialStorageService.StoreRefreshToken(args.Token);
		}

		return result;
	}

	/// <inheritdoc />
	public async Task<LoginResult> Login(Login login)
	{
		var result = await _gnomeshadeClient.LogInAsync(login);
		if (result is SuccessfulLogin)
		{
			_credentialStorageService.StoreCredentials(login.Username, login.Password);
		}

		return result;
	}

	/// <inheritdoc />
	public Task Logout()
	{
		if (_oidcClient is not NullOidcClient)
		{
			// todo probably should revoke the refresh token, but can't find a way to do it with OidcClient
		}

		_gnomeshadeTokenCache.Clear();
		_credentialStorageService.RemoveCredentials();

		return Task.CompletedTask;
	}
}
