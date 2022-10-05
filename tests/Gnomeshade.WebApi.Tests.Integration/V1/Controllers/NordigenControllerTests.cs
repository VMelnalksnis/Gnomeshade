// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Tests.Integration.Fixtures;
using Gnomeshade.WebApi.V1.Controllers;

namespace Gnomeshade.WebApi.Tests.Integration.V1.Controllers;

[TestOf(typeof(NordigenController))]
public sealed class NordigenControllerTests : WebserverTests
{
	private const string _integrationInstitutionId = "SANDBOXFINANCE_SFIN0000";

	public NordigenControllerTests(WebserverFixture fixture)
		: base(fixture)
	{
	}

	[Test]
	public async Task GetInstitutions_ShouldReturnExpected()
	{
		var client = await Fixture.CreateAuthorizedClientAsync();

		var institutions = await client.GetInstitutionsAsync("LV");

		institutions.Should().HaveCount(16);
	}

	[Test]
	public async Task Import_ShouldReturnExpected()
	{
		var client = await Fixture.CreateAuthorizedClientAsync();

		var result = await client.ImportAsync(_integrationInstitutionId);

		result.Should().BeOfType<SuccessfulImport>();
	}
}
