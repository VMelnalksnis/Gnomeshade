// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Gnomeshade.WebApi.Tests.Integration.Fixtures;

public sealed class DatabaseFixtureSource : IEnumerable
{
	internal static readonly List<WebserverFixture> Fixtures = WebserverSetup.WebserverFixtures;

	public IEnumerator GetEnumerator()
	{
		return Fixtures
			.Select(fixture => new TestFixtureData(fixture).SetArgDisplayNames(fixture.Name))
			.GetEnumerator();
	}
}
