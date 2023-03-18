﻿// Copyright 2021 Valters Melnalksnis
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

		result.Should().BeOfType<SuccessfulImport>().Which.Results.Should().HaveCount(2);

		var repeatedResult = await client.ImportAsync(_integrationInstitutionId);

		repeatedResult
			.Should()
			.BeOfType<SuccessfulImport>()
			.Which.Results
			.SelectMany(reportResult => reportResult.TransactionReferences)
			.Should()
			.AllSatisfy(reference => reference.Created.Should().BeFalse());
	}
}
