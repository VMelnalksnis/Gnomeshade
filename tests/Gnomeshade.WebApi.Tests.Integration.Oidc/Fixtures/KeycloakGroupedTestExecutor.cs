// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Gnomeshade.WebApi.Tests.Integration.Oidc.Fixtures;

public sealed class KeycloakGroupedTestExecutor : GroupedTestExecutor<KeycloakFixture>
{
	[After(TestDiscovery)]
	public static void AfterDiscovery(TestDiscoveryContext context)
	{
		Tests = new(Group(context.AllTests));
	}

	/// <inheritdoc />
	protected override bool TryGetKey(TestContext context, [MaybeNullWhen(false)] out KeycloakFixture key)
	{
		if (context.TestDetails.ClassInstance is WebApplicationTests webApplicationTests)
		{
			key = webApplicationTests.Fixture;
			return true;
		}

		key = null;
		return false;
	}

	/// <inheritdoc />
	protected override void BeforeFirstTest(KeycloakFixture fixture) => fixture.PreFirstTest();

	private static IEnumerable<KeyValuePair<KeycloakFixture, List<TestContext>>> Group(
		IEnumerable<TestContext> tests) => tests
		.Where(test => test.TestDetails.ClassInstance is WebApplicationTests)
		.GroupBy(test => ((WebApplicationTests)test.TestDetails.ClassInstance!).Fixture)
		.Select(grouping => new KeyValuePair<KeycloakFixture, List<TestContext>>(grouping.Key, grouping.ToList()));
}
