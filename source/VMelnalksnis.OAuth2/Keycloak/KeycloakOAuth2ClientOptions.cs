// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

namespace VMelnalksnis.OAuth2.Keycloak;

public class KeycloakOAuth2ClientOptions : OAuth2ClientOptions
{
	public KeycloakOAuth2ClientOptions()
	{
		Device = new("protocol/openid-connect/auth/device", UriKind.Relative);
		Token = new("protocol/openid-connect/token", UriKind.Relative);
		Revoke = new("protocol/openid-connect/revoke", UriKind.Relative);
	}
}
