// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Models.Owners;
using Gnomeshade.WebApi.Tests.Integration.Fixtures;
using Gnomeshade.WebApi.V1.Controllers;

namespace Gnomeshade.WebApi.Tests.Integration.V1.Controllers;

[TestOf(typeof(OwnershipsController))]
public sealed class OwnershipsControllerTests : WebserverTests
{
	public OwnershipsControllerTests(WebserverFixture fixture)
		: base(fixture)
	{
	}

	[Test]
	public async Task Get_ShouldContainUserOwnership()
	{
		var client = await Fixture.CreateAuthorizedClientAsync();

		var counterparty = await client.GetMyCounterpartyAsync();
		var ownerships = await client.GetOwnershipsAsync();

		ownerships.Should().ContainSingle(ownership => ownership.Id == counterparty.OwnerId);
	}

	[Test]
	public async Task PutDelete()
	{
		var client = await Fixture.CreateAuthorizedClientAsync();
		var otherClient = await Fixture.CreateAuthorizedClientAsync();

		var counterparty = await client.GetMyCounterpartyAsync();
		var otherCounterparty = await otherClient.GetMyCounterpartyAsync();

		var access = (await client.GetAccessesAsync()).First();

		var ownership = new OwnershipCreation
		{
			OwnerId = counterparty.Id,
			UserId = otherCounterparty.Id,
			AccessId = access.Id,
		};

		var ownershipId = Guid.NewGuid();

		await client.PutOwnershipAsync(ownershipId, ownership);
		await client.DeleteOwnershipAsync(ownershipId);

		(await client.GetOwnershipsAsync()).Should().NotContain(o => o.Id == ownershipId);
	}
}
