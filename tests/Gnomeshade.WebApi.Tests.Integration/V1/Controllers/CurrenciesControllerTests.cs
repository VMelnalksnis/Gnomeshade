// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Tests.Integration.Fixtures;
using Gnomeshade.WebApi.V1.Controllers;

namespace Gnomeshade.WebApi.Tests.Integration.V1.Controllers;

[TestOf(typeof(CurrenciesController))]
public sealed class CurrenciesControllerTests(WebserverFixture fixture) : WebserverTests(fixture)
{
	[Test]
	public async Task ShouldNotContainDuplicates()
	{
		var client = await Fixture.CreateAuthorizedClientAsync();

		var currencies = await client.GetCurrenciesAsync();

		using var scope = new AssertionScope();

		currencies.Should().HaveCount(181);
		currencies
			.GroupBy(currency => currency.AlphabeticCode)
			.Should()
			.AllSatisfy(grouping => grouping.Should().ContainSingle());
	}
}
