// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

namespace Gnomeshade.WebApi.Tests.Integration.Oidc.Fixtures;

public sealed class KeycloakFixtureFormatter : ArgumentDisplayFormatter
{
	/// <inheritdoc />
	public override bool CanHandle(object? value) => value is KeycloakFixture;

	/// <inheritdoc />
	public override string FormatValue(object? value)
	{
		var fixture = (KeycloakFixture)value!;
		return fixture.Name;
	}
}
