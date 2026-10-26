// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Tests.Integration.Fixtures;

[assembly: Parallelizable(ParallelScope.Fixtures)]

namespace Gnomeshade.WebApi.Tests.Integration;

[SetUpFixture]
public static class WebserverSetup
{
	internal static List<WebserverFixture> WebserverFixtures { get; } = new List<PostgreSQLFixture>
	{
		new("18.6-bookworm"),
		new("17.11-bookworm"),
		new("16.15-bookworm"),
		new("15.19-bookworm"),
		new("14.24-bookworm"),
	}.Cast<WebserverFixture>().ToList();

	[OneTimeSetUp]
	public static Task OneTimeSetUpAsync() => Task.WhenAll(WebserverFixtures.Select(fixture => fixture.Initialize()));

	[OneTimeTearDown]
	public static Task OneTimeTearDownAsync() =>
		Task.WhenAll(WebserverFixtures.Select(fixture => fixture.DisposeAsync().AsTask()));
}
