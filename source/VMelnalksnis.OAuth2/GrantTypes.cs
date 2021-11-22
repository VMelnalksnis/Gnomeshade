// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

namespace VMelnalksnis.OAuth2;

internal static class GrantTypes
{
	internal static readonly KeyValuePair<string, string> DeviceCode = new(FieldNames._grantType, _deviceCode);
	internal static readonly KeyValuePair<string, string> RefreshToken = new(FieldNames._refreshToken, _refreshToken);

	private const string _authorizationCode = "authorization_code";
	private const string _implicit = "implicit";
	private const string _refreshToken = "refresh_token";
	private const string _password = "password";
	private const string _clientCredentials = "client_credentials";
	private const string _deviceCode = "urn:ietf:params:oauth:grant-type:device_code";
	private const string _ciba = "urn:openid:params:grant-type:ciba";
}
