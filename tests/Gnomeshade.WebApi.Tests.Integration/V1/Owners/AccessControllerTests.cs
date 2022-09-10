// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Threading.Tasks;

using Gnomeshade.WebApi.Tests.Integration.Fixtures;
using Gnomeshade.WebApi.V1.Owners;

namespace Gnomeshade.WebApi.Tests.Integration.V1.Owners;

[TestOf(typeof(AccessController))]
public sealed class AccessControllerTests : WebserverTests
{
	public AccessControllerTests(WebserverFixture fixture)
		: base(fixture)
	{
	}

	[Test]
	public async Task Get_ShouldReturnExpected()
	{
		var client = await Fixture.CreateAuthorizedClientAsync();
		var otherClient = await Fixture.CreateAuthorizedSecondClientAsync();

		var accesses = await client.GetAccessesAsync();
		accesses.Should().HaveCount(4);

		var otherAccess = await otherClient.GetAccessesAsync();
		otherAccess.Should().BeEquivalentTo(accesses);
	}
}
