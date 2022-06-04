// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.V1_0.Owners;

namespace Gnomeshade.Interfaces.WebApi.Tests.Integration.V1_0.Owners;

[TestOf(typeof(AccessController))]
public sealed class AccessControllerTests
{
	[Test]
	public async Task Get_ShouldReturnExpected()
	{
		var client = await WebserverSetup.CreateAuthorizedClientAsync();
		var otherClient = await WebserverSetup.CreateAuthorizedSecondClientAsync();

		var accesses = await client.GetAccessesAsync();
		accesses.Should().HaveCount(4);

		var otherAccess = await otherClient.GetAccessesAsync();
		otherAccess.Should().BeEquivalentTo(accesses);
	}
}
