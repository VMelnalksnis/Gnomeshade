// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;

using VMelnalksnis.OAuth2;
using VMelnalksnis.OAuth2.Responses;

namespace Gnomeshade.Interfaces.Avalonia.Core.DesignTime;

/// <summary>
/// An implementation of <see cref="IOAuth2Client"/> for use during design time.
/// </summary>
public sealed class DesignTimeOAuth2Client : IOAuth2Client
{
	/// <inheritdoc />
	public Task<DeviceAuthorizationResponse> StartDeviceFlowAsync(CancellationToken cancellationToken = default) =>
		throw new NotImplementedException();

	/// <inheritdoc />
	public Task<DeviceAuthorizationResponse> StartDeviceFlowAsync(
		string scope,
		CancellationToken cancellationToken = default) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task<TokenResponse> GetDeviceFlowResultAsync(
		DeviceAuthorizationResponse deviceAuthorizationResponse,
		CancellationToken cancellationToken = default) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task<TokenResponse> RefreshTokenAsync(
		TokenResponse tokenResponse,
		CancellationToken cancellationToken = default) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task RevokeTokenAsync(string token, CancellationToken cancellationToken = default) =>
		throw new NotImplementedException();
}
