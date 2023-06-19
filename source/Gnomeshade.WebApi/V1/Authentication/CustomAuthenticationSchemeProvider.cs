// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Configuration;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Gnomeshade.WebApi.V1.Authentication;

/// <inheritdoc />
internal sealed class CustomAuthenticationSchemeProvider : AuthenticationSchemeProvider
{
	private readonly IHttpContextAccessor _accessor;
	private readonly JwtSecurityTokenHandler _tokenHandler;
	private readonly Dictionary<string, string> _issuers;

	internal CustomAuthenticationSchemeProvider(
		IOptions<AuthenticationOptions> options,
		IHttpContextAccessor accessor,
		JwtSecurityTokenHandler tokenHandler,
		Dictionary<string, string> issuers)
		: base(options)
	{
		_accessor = accessor;
		_tokenHandler = tokenHandler;
		_issuers = issuers;
	}

	private HttpRequest Request => _accessor.HttpContext?.Request ?? throw new InvalidOperationException();

	/// <inheritdoc />
	public override async Task<AuthenticationScheme?> GetDefaultAuthenticateSchemeAsync() =>
		await GetRequestSchemeAsync() ??
		await base.GetDefaultAuthenticateSchemeAsync();

	/// <inheritdoc />
	public override async Task<AuthenticationScheme?> GetDefaultChallengeSchemeAsync() =>
		await GetRequestSchemeAsync() ??
		await base.GetDefaultChallengeSchemeAsync();

	/// <inheritdoc />
	public override async Task<AuthenticationScheme?> GetDefaultForbidSchemeAsync() =>
		await GetRequestSchemeAsync() ??
		await base.GetDefaultForbidSchemeAsync();

	/// <inheritdoc />
	public override async Task<AuthenticationScheme?> GetDefaultSignInSchemeAsync() =>
		await GetRequestSchemeAsync() ??
		await base.GetDefaultSignInSchemeAsync();

	/// <inheritdoc />
	public override async Task<AuthenticationScheme?> GetDefaultSignOutSchemeAsync() =>
		await GetRequestSchemeAsync() ??
		await base.GetDefaultSignOutSchemeAsync();

	private async Task<AuthenticationScheme?> GetRequestSchemeAsync()
	{
		var request = Request;

		if (!request.IsApiRequest())
		{
			return null;
		}

		string? authorization = request.Headers[HeaderNames.Authorization];
		if (string.IsNullOrWhiteSpace(authorization) || !authorization.StartsWith(Schemes.Bearer, StringComparison.OrdinalIgnoreCase))
		{
			return await GetSchemeAsync(Schemes.Bearer);
		}

		var token = authorization[Schemes.Bearer.Length..].TrimStart();
		if (!_tokenHandler.CanReadToken(token))
		{
			return await GetSchemeAsync(Schemes.Bearer);
		}

		var jwtToken = _tokenHandler.ReadToken(token);
		if (_issuers.TryGetValue(jwtToken.Issuer, out var scheme))
		{
			return await GetSchemeAsync(scheme);
		}

		return await GetSchemeAsync(Schemes.Bearer);
	}
}
