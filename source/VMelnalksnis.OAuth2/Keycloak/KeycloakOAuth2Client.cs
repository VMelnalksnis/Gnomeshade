// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace VMelnalksnis.OAuth2.Keycloak;

public sealed class KeycloakOAuth2Client : OAuth2Client
{
	public KeycloakOAuth2Client(
		HttpClient httpClient,
		IOptionsMonitor<KeycloakOAuth2ClientOptions> clientOptionsMonitor,
		ILogger<KeycloakOAuth2Client> logger)
		: base(httpClient, clientOptionsMonitor, logger)
	{
	}
}
