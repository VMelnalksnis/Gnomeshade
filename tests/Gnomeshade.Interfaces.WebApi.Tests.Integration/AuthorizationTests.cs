// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Net;
using System.Threading.Tasks;

namespace Gnomeshade.Interfaces.WebApi.Tests.Integration;

public class AuthorizationTests
{
	[Test]
	public async Task Get_ShouldReturnUnauthorized()
	{
		var client = WebserverSetup.CreateHttpClient();

		var response = await client.GetAsync("/api/v1.0/transactions");

		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
	}
}
