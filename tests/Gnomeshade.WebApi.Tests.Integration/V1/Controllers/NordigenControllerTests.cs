// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Linq;
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

		institutions.Should().ContainEquivalentOf("CITADELE_PARXLV22");
	}

	[Test]
	public async Task Import_ShouldReturnExpected()
	{
		var client = await Fixture.CreateAuthorizedClientAsync();

		var result = await client.ImportAsync(_integrationInstitutionId);

		var reportResults = result.Should().BeOfType<SuccessfulImport>().Subject.Results;

		using (new AssertionScope())
		{
			reportResults.Should().HaveCount(2);

			reportResults
				.First()
				.TransferReferences.Should()
				.AllSatisfy(transfer => transfer.Transfer.SourceAmount.Should().Be(transfer.Transfer.TargetAmount));

			reportResults
				.First()
				.AccountReferences.Should()
				.AllSatisfy(reference => reference.Created.Should().BeTrue());
		}

		var repeatedResult = await client.ImportAsync(_integrationInstitutionId);

		var secondReportResults = repeatedResult.Should().BeOfType<SuccessfulImport>().Subject.Results;
		using (new AssertionScope())
		{
			secondReportResults.Should().HaveCount(2);

			secondReportResults
				.SelectMany(reportResult => reportResult.TransactionReferences)
				.Should()
				.AllSatisfy(reference => reference.Created.Should().BeFalse());

			secondReportResults
				.First()
				.AccountReferences.Should()
				.AllSatisfy(reference => reference.Created.Should().BeFalse());
		}
	}
}
