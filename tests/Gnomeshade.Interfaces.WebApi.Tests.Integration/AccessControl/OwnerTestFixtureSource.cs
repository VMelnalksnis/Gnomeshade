// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Linq;

using Gnomeshade.Interfaces.WebApi.Models.Owners;

namespace Gnomeshade.Interfaces.WebApi.Tests.Integration.AccessControl;

internal sealed class OwnerTestFixtureSource : IEnumerable
{
	public IEnumerator GetEnumerator()
	{
		yield return new TestFixtureData(async () =>
		{
			var client = await WebserverSetup.CreateAuthorizedClientAsync();
			var counterparty = await client.GetMyCounterpartyAsync();
			return counterparty.OwnerId;
		}).SetArgDisplayNames("Original owner");

		yield return new TestFixtureData(async () =>
		{
			var client = await WebserverSetup.CreateAuthorizedClientAsync();
			var counterparty = await client.GetMyCounterpartyAsync();
			var ownerAccess = (await client.GetAccessesAsync()).Single(access => access.Name == "Owner");
			var ownerId = Guid.NewGuid();

			await client.PutOwnerAsync(ownerId);
			var ownership = new OwnershipCreation
			{
				AccessId = ownerAccess.Id,
				OwnerId = ownerId,
				UserId = counterparty.OwnerId,
			};

			await client.PutOwnershipAsync(Guid.NewGuid(), ownership);

			return ownerId;
		}).SetArgDisplayNames("Additional owner");
	}
}
