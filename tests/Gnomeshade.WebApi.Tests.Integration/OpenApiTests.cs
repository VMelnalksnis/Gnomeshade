// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Threading.Tasks;

using Gnomeshade.WebApi.Tests.Integration.Fixtures;

namespace Gnomeshade.WebApi.Tests.Integration;

[Parallelizable(ParallelScope.None)]
public sealed class OpenApiTests(WebserverFixture fixture) : WebserverTests(fixture)
{
	[TestCase("/swagger/v1/swagger.json")]
	[TestCase("/swagger/v2/swagger.json")]
	public async Task ApiDefinition_ShouldBeExpected(string path)
	{
		using var apiClient = Fixture.CreateHttpClient();

		using var response = await apiClient.GetAsync(path);

		var content = await response.Content.ReadAsStringAsync();

		await Verifier
			.VerifyJson(content)
			.UseFileName($"{nameof(OpenApiTests)}.{nameof(ApiDefinition_ShouldBeExpected)}.{path.Replace('/', '_')}")
			.DisableRequireUniquePrefix();
	}
}
