// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;

using IdentityModel.Client;
using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Results;

namespace Gnomeshade.WebApi.Client;

/// <summary>An <see cref="OidcClient"/> that does nothing.</summary>
public sealed class NullOidcClient : OidcClient
{
	/// <summary>Initializes a new instance of the <see cref="NullOidcClient"/> class.</summary>
	public NullOidcClient()
		: base(new() { Authority = "https://localhost/" })
	{
	}

	/// <inheritdoc />
	public override Task<IdentityModel.OidcClient.LoginResult> LoginAsync(
		LoginRequest request = null!,
		CancellationToken cancellationToken = default)
	{
		throw new NotSupportedException();
	}

	/// <inheritdoc />
	public override Task<LogoutResult> LogoutAsync(
		LogoutRequest request = null!,
		CancellationToken cancellationToken = default)
	{
		throw new NotSupportedException();
	}

	/// <inheritdoc />
	public override Task<AuthorizeState> PrepareLoginAsync(
		Parameters frontChannelParameters = null!,
		CancellationToken cancellationToken = default)
	{
		throw new NotSupportedException();
	}

	/// <inheritdoc />
	public override Task<string> PrepareLogoutAsync(
		LogoutRequest request = null!,
		CancellationToken cancellationToken = default)
	{
		throw new NotSupportedException();
	}

	/// <inheritdoc />
	public override Task<IdentityModel.OidcClient.LoginResult> ProcessResponseAsync(
		string data,
		AuthorizeState state,
		Parameters backChannelParameters = null!,
		CancellationToken cancellationToken = default)
	{
		throw new NotSupportedException();
	}

	/// <inheritdoc />
	public override Task<RefreshTokenResult> RefreshTokenAsync(
		string refreshToken,
		Parameters backChannelParameters = null!,
		string scope = null!,
		CancellationToken cancellationToken = default)
	{
		throw new NotSupportedException();
	}

	/// <inheritdoc />
	public override Task<UserInfoResult> GetUserInfoAsync(
		string accessToken,
		CancellationToken cancellationToken = default)
	{
		throw new NotSupportedException();
	}
}
