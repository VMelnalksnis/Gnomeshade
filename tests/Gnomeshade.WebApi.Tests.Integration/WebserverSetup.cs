// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

using Bogus;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Authentication;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

using NodaTime;

namespace Gnomeshade.WebApi.Tests.Integration;

[SetUpFixture]
public sealed class WebserverSetup
{
	private static readonly IConfiguration _configuration;

	private static readonly PostgreSqlTestcontainer _postgreSqlTestcontainer =
		new TestcontainersBuilder<PostgreSqlTestcontainer>()
			.WithDatabase(new PostgreSqlTestcontainerConfiguration
			{
				Database = "gnomeshade-test",
				Username = "gnomeshade",
				Password = "foobar",
			})
			.WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
			.Build();

	private static readonly PostgreSqlTestcontainer _postgreSqlTestIdentitycontainer =
		new TestcontainersBuilder<PostgreSqlTestcontainer>()
			.WithDatabase(new PostgreSqlTestcontainerConfiguration
			{
				Database = "gnomeshade-identity-test",
				Username = "gnomeshade",
				Password = "foobar",
			})
			.WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
			.Build();

	private static WebApplicationFactory<Startup> _webApplicationFactory = null!;
	private static Login _login = null!;
	private static Login _secondLogin = null!;

	static WebserverSetup()
	{
		Task.WhenAll(_postgreSqlTestcontainer.StartAsync(), _postgreSqlTestIdentitycontainer.StartAsync())
			.GetAwaiter()
			.GetResult();

		_configuration =
			new ConfigurationBuilder()
				.AddInMemoryCollection(new Dictionary<string, string>
				{
					{ "ConnectionStrings:FinanceDb", _postgreSqlTestcontainer.ConnectionString },
					{ "ConnectionStrings:IdentityDb", _postgreSqlTestIdentitycontainer.ConnectionString },
				})
				.AddEnvironmentVariables()
				.Build();
	}

	public static HttpClient CreateHttpClient() => _webApplicationFactory.CreateClient();

	public static GnomeshadeClient CreateUnauthorizedClient()
	{
		var httpClient = CreateHttpClient();
		httpClient.BaseAddress = new("https://localhost:5001/api/v1.0/");
		return new(httpClient, new(DateTimeZoneProviders.Tzdb));
	}

	public static Task<IGnomeshadeClient> CreateAuthorizedClientAsync() => CreateAuthorizedClientAsync(_login);

	public static Task<IGnomeshadeClient> CreateAuthorizedSecondClientAsync() =>
		CreateAuthorizedClientAsync(_secondLogin);

	[OneTimeSetUp]
	public async Task OneTimeSetUpAsync()
	{
		_webApplicationFactory = new GnomeshadeWebApplicationFactory(_configuration);
		var client = _webApplicationFactory.CreateClient();
		var registrationFaker = new Faker<RegistrationModel>()
			.RuleFor(registration => registration.Email, faker => faker.Internet.Email())
			.RuleFor(registration => registration.Password, faker => faker.Internet.Password(10, 12))
			.RuleFor(registration => registration.Username, faker => faker.Internet.UserName())
			.RuleFor(registration => registration.FullName, faker => faker.Person.FullName);

		_login = await RegisterUser(client, registrationFaker.Generate());
		_secondLogin = await RegisterUser(client, registrationFaker.Generate());
	}

	[OneTimeTearDown]
	public async Task OneTimeTearDownAsync()
	{
		await _webApplicationFactory.DisposeAsync();
	}

	private static async Task<Login> RegisterUser(HttpClient client, RegistrationModel registrationModel)
	{
		var response = await client.PostAsJsonAsync("api/v1.0/authentication/register", registrationModel);
		if (response.StatusCode is HttpStatusCode.InternalServerError)
		{
			throw new(await response.Content.ReadAsStringAsync());
		}

		response.EnsureSuccessStatusCode();

		return new() { Username = registrationModel.Username, Password = registrationModel.Password };
	}

	private static async Task<IGnomeshadeClient> CreateAuthorizedClientAsync(Login login)
	{
		var client = CreateUnauthorizedClient();
		var loginResult = await client.LogInAsync(login);
		if (loginResult is not SuccessfulLogin)
		{
			throw new(loginResult.ToString());
		}

		return client;
	}
}
