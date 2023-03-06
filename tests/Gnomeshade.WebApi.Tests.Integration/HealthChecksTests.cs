// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Net;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Tests.Integration.Fixtures;

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Gnomeshade.WebApi.Tests.Integration;

public sealed class HealthChecksTests : WebserverTests
{
	public HealthChecksTests(WebserverFixture fixture)
		: base(fixture)
	{
	}

	[Test]
	public async Task Health_ShouldReturnHealthy()
	{
		var apiClient = Fixture.CreateHttpClient();

		using var response = await apiClient.GetAsync("/api/v1.0/Health");
		var content = await response.Content.ReadAsStringAsync();

		using (new AssertionScope())
		{
			response.StatusCode.Should().Be(HttpStatusCode.OK);
			response.Headers.CacheControl?.NoCache.Should().BeTrue();
			content.Should().Be(nameof(HealthStatus.Healthy));
		}
	}
}
