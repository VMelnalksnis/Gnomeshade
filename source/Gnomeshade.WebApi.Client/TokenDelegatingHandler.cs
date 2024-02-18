// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Models;

using IdentityModel.OidcClient;

namespace Gnomeshade.WebApi.Client;

/// <summary>Handles authorization tokens for <see cref="IGnomeshadeClient"/>.</summary>
public sealed class TokenDelegatingHandler : DelegatingHandler
{
	private readonly GnomeshadeTokenCache _tokenCache;
	private readonly GnomeshadeSerializerContext _context;
	private readonly OidcClient _oidcClient;

	/// <summary>Initializes a new instance of the <see cref="TokenDelegatingHandler"/> class.</summary>
	/// <param name="tokenCache">Gnomeshade API token cache for preserving tokens between requests.</param>
	/// <param name="jsonSerializerOptions">Gnomeshade specific instance of <see cref="JsonSerializerOptions"/>.</param>
	/// <param name="oidcClient">OIDC client for token management.</param>
	public TokenDelegatingHandler(
		GnomeshadeTokenCache tokenCache,
		GnomeshadeJsonSerializerOptions jsonSerializerOptions,
		OidcClient oidcClient)
	{
		_tokenCache = tokenCache;
		_context = jsonSerializerOptions.Context;
		_oidcClient = oidcClient;
	}

	/// <inheritdoc />
	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		if (request.RequestUri?.ToString().EndsWith(Routes.LoginUri) ?? false)
		{
			return HandleBuiltInUsers(request, cancellationToken);
		}

		return HandleOAuth(request, cancellationToken);
	}

	private async Task<HttpResponseMessage> HandleBuiltInUsers(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		var loginResponse = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
		if (loginResponse.StatusCode is not HttpStatusCode.OK)
		{
			return loginResponse;
		}

		var login = await loginResponse.Content
			.ReadFromJsonAsync(_context.LoginResponse, cancellationToken)
			.ConfigureAwait(false);

		_tokenCache.SetRefreshToken(login!.Token, login.Token, login.ValidTo.ToDateTimeOffset());

		loginResponse.Content = new StringContent(JsonSerializer.Serialize(login, _context.LoginResponse), Encoding.UTF8, MediaTypeNames.Application.Json);
		return loginResponse;
	}

	private async Task<HttpResponseMessage> HandleOAuth(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		if (_tokenCache.Access is null)
		{
			if (_tokenCache.Refresh is null)
			{
				await CreateNewToken(cancellationToken).ConfigureAwait(false);
			}
			else
			{
				await RefreshToken(cancellationToken).ConfigureAwait(false);
			}
		}
		else if (_tokenCache.IsAccessExpired)
		{
			await RefreshToken(cancellationToken).ConfigureAwait(false);
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
		var loginResult = await _oidcClient.LoginAsync(new(), cancellationToken).ConfigureAwait(false);
		_tokenCache.SetRefreshToken(loginResult.RefreshToken, loginResult.AccessToken, loginResult.AccessTokenExpiration);
	}

	private async Task RefreshToken(CancellationToken cancellationToken)
	{
		var refreshTokenResult = await _oidcClient
			.RefreshTokenAsync(_tokenCache.Refresh, null, _oidcClient.Options.Scope, cancellationToken)
			.ConfigureAwait(false);

		_tokenCache.SetRefreshToken(refreshTokenResult.RefreshToken, refreshTokenResult.AccessToken, refreshTokenResult.AccessTokenExpiration);
	}
}
