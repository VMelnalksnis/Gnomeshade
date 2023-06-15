// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Gnomeshade.Avalonia.Core.Authentication;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Client.Results;

using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NodaTime;

namespace Gnomeshade.WebApi.Tests.Integration.Oidc;

[TestFixtureSource(typeof(OidcFixtureSource))]
public sealed class AuthorizationTests
{
	private readonly KeycloakFixture _fixture;

	public AuthorizationTests(KeycloakFixture fixture)
	{
		_fixture = fixture;
	}

	[Test]
	public async Task Get_ShouldReturnUnauthorized()
	{
		var client = _fixture.CreateHttpClient();

		var response = await client.GetAsync("/api/v1.0/transactions");

		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
	}

	[Test]
	public async Task SocialRegister()
	{
		var services = new ServiceCollection();

		services
			.AddHttpClient()
			.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug))
			.AddSingleton<MockBrowser>(provider =>
			{
				var handler = provider.GetRequiredService<IGnomeshadeProtocolHandler>();
				var httpClient = provider.GetRequiredService<HttpClient>();
				var internalClient = _fixture.CreateHttpClient();
				return new(handler, httpClient, internalClient, _fixture.User.Username, _fixture.User.Password);
			})
			.AddSingleton<IBrowser, MockBrowser>(provider => provider.GetRequiredService<MockBrowser>())
			.AddSingleton<IGnomeshadeProtocolHandler, MockProtocolHandler>(_ => new(KeycloakFixture.DesktopBaseUri))
			.AddTransient<OidcClient>(provider => new(new()
			{
				Authority = _fixture.Realm.ServerRealm.ToString(),
				ClientId = _fixture.DesktopClient.Name,
				ClientSecret = _fixture.DesktopClient.Secret,
				Scope = "openid profile",
				RedirectUri = KeycloakFixture.DesktopBaseUri,
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
		var gnomeshadeClient = _fixture.CreateUnauthorizedClient(handler);

		var result = await gnomeshadeClient.SocialRegister();
		var redirectUri = result.Should().BeOfType<RequiresRegistration>().Subject.RedirectUri;

		var mockBrowser = provider.GetRequiredService<MockBrowser>();
		await mockBrowser.RegisterUser(redirectUri.ToString());

		// this works
		var secondResult = await gnomeshadeClient.SocialRegister();
		secondResult.Should().BeOfType<LoggedIn>();

		// this doesn't ???
		var counterparty = await gnomeshadeClient.GetMyCounterpartyAsync();
		counterparty.Name.Should().Be("John Doe");
	}
}
