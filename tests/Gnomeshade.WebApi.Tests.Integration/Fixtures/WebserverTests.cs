// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

namespace Gnomeshade.WebApi.Tests.Integration.Fixtures;

[TestFixtureSource(typeof(DatabaseFixtureSource))]
public abstract class WebserverTests
{
	protected WebserverTests(WebserverFixture fixture)
	{
		Fixture = fixture;
	}

	protected WebserverFixture Fixture { get; }
}
