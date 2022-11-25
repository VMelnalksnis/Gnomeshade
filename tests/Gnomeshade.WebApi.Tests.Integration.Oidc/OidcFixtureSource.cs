// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections;

namespace Gnomeshade.WebApi.Tests.Integration.Oidc;

internal sealed class OidcFixtureSource : IEnumerable
{
	public IEnumerator GetEnumerator()
	{
		yield return new TestFixtureData(WebserverSetup.KeycloakFixture).SetArgDisplayNames("Keycloak");
	}
}
