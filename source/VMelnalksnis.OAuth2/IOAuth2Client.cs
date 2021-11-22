// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using VMelnalksnis.OAuth2.Responses;

namespace VMelnalksnis.OAuth2;

public interface IOAuth2Client
{
	Task<DeviceAuthorizationResponse> StartDeviceFlowAsync(
		CancellationToken cancellationToken = default);

	Task<DeviceAuthorizationResponse> StartDeviceFlowAsync(
		string scope,
		CancellationToken cancellationToken = default);

	Task<TokenResponse> GetDeviceFlowResultAsync(
		DeviceAuthorizationResponse deviceAuthorizationResponse,
		CancellationToken cancellationToken = default);

	Task<TokenResponse> RefreshTokenAsync(
		TokenResponse tokenResponse,
		CancellationToken cancellationToken = default);

	Task RevokeTokenAsync(string token, CancellationToken cancellationToken = default);
}
