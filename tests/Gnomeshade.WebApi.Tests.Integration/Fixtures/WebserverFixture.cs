﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using Microsoft.Extensions.DependencyInjection;

using NodaTime;

namespace Gnomeshade.WebApi.Tests.Integration.Fixtures;

public abstract class WebserverFixture : IAsyncDisposable
{
	private const int _keycloakPort = 8080;
	private WebApplicationFactory<Startup> _webApplicationFactory = null!;
	private Login _login = null!;
	private Login _secondLogin = null!;

	internal abstract string Name { get; }

	internal int KeycloakPort { get; private set; }

	internal abstract int RedirectPort { get; }

	protected List<ITestcontainersContainer> Containers { get; } = new();

	public async ValueTask DisposeAsync()
	{
		await _webApplicationFactory.DisposeAsync();
		await Task.WhenAll(Containers.Select(container => container.StopAsync()));
	}

	internal async Task Initialize()
	{
		var keycloakContainer = new TestcontainersBuilder<TestcontainersContainer>()
			.WithImage("quay.io/keycloak/keycloak:19.0.1")
			.WithEnvironment(new Dictionary<string, string>
			{
				{ "KEYCLOAK_ADMIN", "admin" },
				{ "KEYCLOAK_ADMIN_PASSWORD", "admin" },
			})
			.WithPortBinding(_keycloakPort, true)
			.WithBindMount(
				Path.Combine(Directory.GetCurrentDirectory(), "realm-export.json"),
				"/opt/keycloak/data/import/realm.json",
				AccessMode.ReadOnly)
			.WithCommand("start-dev", "--import-realm")
			.WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8080))
			.Build();

		Containers.Add(keycloakContainer);

		await Task.WhenAll(Containers.Select(container => container.StartAsync()));

		KeycloakPort = keycloakContainer.GetMappedPublicPort(_keycloakPort);
		var configuration =
			new ConfigurationBuilder()
				.AddConfiguration(GetAdditionalConfiguration())
				.AddInMemoryCollection(new Dictionary<string, string>
				{
					{ "Oidc:Keycloak:ServerRealm", $"http://localhost:{KeycloakPort}/realms/gnomeshade" },
					{ "Oidc:Keycloak:Metadata", $"http://localhost:{KeycloakPort}/realms/gnomeshade/.well-known/openid-configuration" },
					{ "Oidc:Keycloak:ClientId", "gnomeshade" },
				})
				.AddEnvironmentVariables()
				.Build();

		_webApplicationFactory = new GnomeshadeWebApplicationFactory(configuration);

		var client = _webApplicationFactory.CreateClient();
		var registrationFaker = new Faker<RegistrationModel>()
			.RuleFor(registration => registration.Email, faker => faker.Internet.Email())
			.RuleFor(registration => registration.Password, faker => faker.Internet.Password(10, 12))
			.RuleFor(registration => registration.Username, faker => faker.Internet.UserName())
			.RuleFor(registration => registration.FullName, faker => faker.Person.FullName);

		_login = await RegisterUser(client, registrationFaker.Generate());
		_secondLogin = await RegisterUser(client, registrationFaker.Generate());
	}

	internal HttpClient CreateHttpClient(params DelegatingHandler[] handlers) =>
		_webApplicationFactory.CreateDefaultClient(handlers);

	internal GnomeshadeClient CreateUnauthorizedClient(params DelegatingHandler[] handlers)
	{
		var httpClient = CreateHttpClient(handlers);
		return new(httpClient, new(DateTimeZoneProviders.Tzdb));
	}

	internal Task<IGnomeshadeClient> CreateAuthorizedClientAsync() => CreateAuthorizedClientAsync(_login);

	internal Task<IGnomeshadeClient> CreateAuthorizedSecondClientAsync() =>
		CreateAuthorizedClientAsync(_secondLogin);

	internal IServiceScope CreateScope() => _webApplicationFactory.Services.CreateScope();

	internal EntityRepository GetEntityRepository(IServiceScope scope) =>
		scope.ServiceProvider.GetRequiredService<EntityRepository>();

	protected abstract IConfiguration GetAdditionalConfiguration();

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

	private async Task<IGnomeshadeClient> CreateAuthorizedClientAsync(Login login)
	{
		var client = CreateUnauthorizedClient(new TokenDelegatingHandler(new(SystemClock.Instance), new(DateTimeZoneProviders.Tzdb), null!));
		var loginResult = await client.LogInAsync(login);
		if (loginResult is not SuccessfulLogin)
		{
			throw new(loginResult.ToString());
		}

		return client;
	}
}
