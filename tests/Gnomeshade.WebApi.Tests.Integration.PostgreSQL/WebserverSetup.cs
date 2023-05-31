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
		new("16beta1"),
		new("15"),
		new("14"),
		new("13"),
		new("12"),
		new("11-bullseye"),
	}.Cast<WebserverFixture>().ToList();

	[OneTimeSetUp]
	public static Task OneTimeSetUpAsync() => Task.WhenAll(WebserverFixtures.Select(fixture => fixture.Initialize()));

	[OneTimeTearDown]
	public static Task OneTimeTearDownAsync() =>
		Task.WhenAll(WebserverFixtures.Select(fixture => fixture.DisposeAsync().AsTask()));
}
