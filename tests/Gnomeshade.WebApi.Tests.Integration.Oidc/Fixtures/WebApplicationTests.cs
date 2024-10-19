// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

namespace Gnomeshade.WebApi.Tests.Integration.Oidc.Fixtures;

public abstract class WebApplicationTests
{
	protected WebApplicationTests(KeycloakFixture fixture)
	{
		Fixture = fixture;
	}

	public KeycloakFixture Fixture { get; init; }
}
