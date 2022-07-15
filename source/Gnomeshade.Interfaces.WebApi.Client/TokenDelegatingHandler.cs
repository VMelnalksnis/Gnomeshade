// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using IdentityModel.OidcClient;

namespace Gnomeshade.Interfaces.WebApi.Client;

/// <summary>Handles authorization tokens for <see cref="IGnomeshadeClient"/>.</summary>
public sealed class TokenDelegatingHandler : DelegatingHandler
{
	private readonly GnomeshadeTokenCache _tokenCache;
	private readonly OidcClient _oidcClient;

	/// <summary>Initializes a new instance of the <see cref="TokenDelegatingHandler"/> class.</summary>
	/// <param name="tokenCache">Gnomeshade API token cache for preserving tokens between requests.</param>
	/// <param name="oidcClient">OIDC client for token management.</param>
	public TokenDelegatingHandler(GnomeshadeTokenCache tokenCache, OidcClient oidcClient)
	{
		_tokenCache = tokenCache;
		_oidcClient = oidcClient;
	}

	/// <inheritdoc />
	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		if (_tokenCache.Access is null)
		{
			await CreateNewToken(cancellationToken).ConfigureAwait(false);
		}
		else if (_tokenCache.IsAccessExpired)
		{
			var refreshTokenResult = await Task
				.Run(() => _oidcClient.RefreshTokenAsync(_tokenCache.Refresh, null, cancellationToken), cancellationToken)
				.ConfigureAwait(false);

			_tokenCache.SetAccessToken(refreshTokenResult.AccessToken, refreshTokenResult.ExpiresIn);
		}

		request.Headers.Authorization = new("Bearer", _tokenCache.Access);
		var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
		if (response.StatusCode is not HttpStatusCode.Unauthorized)
		{
			return response;
		}

		await CreateNewToken(cancellationToken).ConfigureAwait(false);
		request.Headers.Authorization = new("Bearer", _tokenCache.Access);

		return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
	}

	private async Task CreateNewToken(CancellationToken cancellationToken)
	{
		var loginResult = await Task
			.Run(() => _oidcClient.LoginAsync(new(), cancellationToken), cancellationToken)
			.ConfigureAwait(false);

		_tokenCache.SetRefreshToken(loginResult.RefreshToken, loginResult.AccessToken, loginResult.AccessTokenExpiration);
	}
}
