// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Linq;

using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Owners;
using Gnomeshade.WebApi.Tests.Integration.Fixtures;

namespace Gnomeshade.WebApi.Tests.Integration.AccessControl;

internal sealed class OwnerTestFixtureSource : IEnumerable
{
	public IEnumerator GetEnumerator()
	{
		foreach (var webserverFixture in DatabaseFixtureSource.Fixtures)
		{
			yield return new TestFixtureData(
					async (IGnomeshadeClient client) =>
					{
						var counterparty = await client.GetMyCounterpartyAsync();
						return counterparty.OwnerId;
					},
					webserverFixture)
				.SetArgDisplayNames("Original owner", webserverFixture.Name);

			yield return new TestFixtureData(
					async (IGnomeshadeClient client) =>
					{
						var counterparty = await client.GetMyCounterpartyAsync();
						var ownerAccess = (await client.GetAccessesAsync()).Single(access => access.Name == "Owner");
						var ownerId = Guid.NewGuid();

						await client.PutOwnerAsync(ownerId, new() { Name = "Test owner" });
						var ownership = new OwnershipCreation
						{
							AccessId = ownerAccess.Id,
							OwnerId = ownerId,
							UserId = counterparty.OwnerId,
						};

						await client.PutOwnershipAsync(Guid.NewGuid(), ownership);

						return ownerId;
					},
					webserverFixture)
				.SetArgDisplayNames("Additional owner", webserverFixture.Name);
		}
	}
}
