// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Net;
using System.Threading.Tasks;

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Gnomeshade.Interfaces.WebApi.Tests.Integration;

public class HealthChecksTests
{
	[Test]
	public async Task Health_ShouldReturnHealthy()
	{
		var apiClient = WebserverSetup.CreateHttpClient();

		using var response = await apiClient.GetAsync("/health");
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var content = await response.Content.ReadAsStringAsync();
		content.Should().Be(nameof(HealthStatus.Healthy));
	}
}
