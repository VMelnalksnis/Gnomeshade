// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using DotNet.Testcontainers.Builders;

using Gnomeshade.WebApi.Client;

using Microsoft.Extensions.Configuration;

using NodaTime;

using VMelnalksnis.Testcontainers.Keycloak;
using VMelnalksnis.Testcontainers.Keycloak.Configuration;

using KeycloakClient = VMelnalksnis.Testcontainers.Keycloak.Configuration.Client;

namespace Gnomeshade.WebApi.Tests.Integration.Oidc;

public sealed class KeycloakFixture : IAsyncDisposable
{
	internal const int Port = 8297;
	private const string _databasePath = "gnomeshade.db";

	private readonly KeycloakTestcontainer _keycloak;
	private GnomeshadeWebApplicationFactory _application = null!;

	public KeycloakFixture()
	{
		File.Delete(_databasePath);

		var mapper = new ClientProtocolMapper("audience-mapping", "openid-connect", "oidc-audience-mapper");
		Client = new("gnomeshade", new($"http://localhost:{Port}/"))
		{
			Mappers = new[] { mapper },
			Secret = Guid.NewGuid().ToString(),
		};

		var realmConfiguration = new RealmConfiguration(
			"demorealm",
			new List<KeycloakClient> { Client },
			new List<User> { User });

		_keycloak = new TestcontainersBuilder<KeycloakTestcontainer>()
			.WithKeycloak(new() { Realms = new[] { realmConfiguration } })
			.Build();
	}

	internal Realm Realm { get; private set; } = null!;

	internal KeycloakClient Client { get; private set; }

	internal User User { get; } = new("john.doe", "password123")
	{
		Email = "john.doe@example.com",
		EmailVerified = true,
		FirstName = "John",
		LastName = "Doe",
	};

	public async Task Initialize()
	{
		await _keycloak.StartAsync();

		Realm = _keycloak.Realms.Single();
		var configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				{ "ConnectionStrings:Gnomeshade", $"Data Source={_databasePath}" },
				{ "Database:Provider", "Sqlite" },
				{ "Oidc:Keycloak:ServerRealm", Realm.ServerRealm.ToString() },
				{ "Oidc:Keycloak:Metadata", Realm.Metadata.ToString() },
				{ "Oidc:Keycloak:ClientId", Client.Name },
				{ "Oidc:Keycloak:ClientSecret", Client.Secret },
				{ "Oidc:Keycloak:RequireHttpsMetadata", "false" },
				{ "Jwt:ValidAudience", $"http://localhost:{Port}/" },
				{ "Jwt:ValidIssuer", $"http://localhost:{Port}/" },
				{ "Jwt:Secret", Guid.NewGuid().ToString() },
			})
			.AddEnvironmentVariables()
			.Build();

		_application = new(configuration);
		_application.CreateClient();
	}

	public ValueTask DisposeAsync()
	{
		return ValueTask.CompletedTask;
	}

	internal HttpClient CreateHttpClient(params DelegatingHandler[] handlers) =>
		_application.CreateDefaultClient(handlers);

	internal GnomeshadeClient CreateUnauthorizedClient(params DelegatingHandler[] handlers)
	{
		var httpClient = CreateHttpClient(handlers);
		return new(httpClient, new(DateTimeZoneProviders.Tzdb));
	}
}
