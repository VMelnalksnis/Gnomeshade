// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Tests.Integration.Fixtures;

using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NodaTime;

namespace Gnomeshade.WebApi.Tests.Integration;

public sealed class AuthorizationTests : WebserverTests
{
	public AuthorizationTests(WebserverFixture fixture)
		: base(fixture)
	{
	}

	[Test]
	public async Task Get_ShouldReturnUnauthorized()
	{
		var client = Fixture.CreateHttpClient();

		var response = await client.GetAsync("/api/v1.0/transactions");

		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
	}

	[Test]
	public async Task SocialRegister()
	{
		var services = new ServiceCollection();

		services
			.AddHttpClient()
			.AddLogging()
			.AddSingleton<IBrowser, MockBrowser>()
			.AddTransient<OidcClient>(provider => new(new()
			{
				Authority = $"http://localhost:{Fixture.KeycloakPort}/realms/gnomeshade/",
				ClientId = "gnomeshade",
				Scope = "openid profile",
				RedirectUri = $"http://localhost:{Fixture.RedirectPort}/",
				Browser = provider.GetRequiredService<IBrowser>(),
				LoggerFactory = provider.GetRequiredService<ILoggerFactory>(),
				HttpClientFactory = _ => provider.GetRequiredService<HttpClient>(),
			}))
			.AddSingleton<IClock>(SystemClock.Instance)
			.AddSingleton(DateTimeZoneProviders.Tzdb)
			.AddSingleton<GnomeshadeTokenCache>()
			.AddSingleton<GnomeshadeJsonSerializerOptions>()
			.AddTransient<TokenDelegatingHandler>();

		var provider = services.BuildServiceProvider();

		var handler = provider.GetRequiredService<TokenDelegatingHandler>();
		var gnomeshadeClient = Fixture.CreateUnauthorizedClient(handler);

		await gnomeshadeClient.SocialRegister();

		var counterparty = await gnomeshadeClient.GetMyCounterpartyAsync();
		counterparty.Name.Should().Be("John Doe");
	}
}
