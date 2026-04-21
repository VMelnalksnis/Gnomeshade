// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;

using Microsoft.Extensions.Configuration;

using NodaTime;

using Testcontainers.Keycloak;

using VMelnalksnis.Testcontainers.Keycloak;
using VMelnalksnis.Testcontainers.Keycloak.Configuration;

using KeycloakClient = VMelnalksnis.Testcontainers.Keycloak.Configuration.Client;

namespace Gnomeshade.WebApi.Tests.Integration.Oidc;

public sealed class KeycloakFixture : IAsyncDisposable
{
	internal const string ApiBaseUri = "https://localhost:5001/";
	internal const string DesktopBaseUri = "http://localhost:8297/";
	private const string _databasePath = "gnomeshade.db";

	private static int _keycloakPublicPort = 45278;

	private readonly KeycloakContainer _keycloak;
	private GnomeshadeWebApplicationFactory _application = null!;

	public KeycloakFixture()
	{
		File.Delete(_databasePath);

		var mapper = new ClientProtocolMapper("audience-mapping", "openid-connect", "oidc-audience-mapper")
		{
			IncludedClientAudience = "gnomeshade",
		};

		Client = new("gnomeshade")
		{
			Mappers = [mapper],
			Secret = Guid.NewGuid().ToString(),
			RedirectUris = [$"{ApiBaseUri}*"],
		};

		DesktopClient = new("gnomeshade_desktop")
		{
			Mappers = [mapper],
			Secret = Guid.NewGuid().ToString(),
			RedirectUris = [DesktopBaseUri],
		};

		RealmConfiguration = new(
			"demorealm",
			new List<KeycloakClient> { Client, DesktopClient },
			new List<User> { User });

		var publicPort = Interlocked.Increment(ref _keycloakPublicPort);

		_keycloak = new KeycloakBuilder("quay.io/keycloak/keycloak:26.5.6")
			.WithUsername("admin")
			.WithPassword("admin")
			.WithPortBinding(publicPort, KeycloakBuilder.KeycloakPort)
			.WithEnvironment("KC_HOSTNAME", $"http://localhost:{publicPort}")
			.WithEnvironment("KC_HOSTNAME_BACKCHANNEL_DYNAMIC", "true")
			.Build();
	}

	internal Realm Realm { get; private set; } = null!;

	internal RealmConfiguration RealmConfiguration { get; }

	internal KeycloakClient Client { get; }

	internal KeycloakClient DesktopClient { get; }

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
		await _keycloak.ConfigureRealm(RealmConfiguration, "admin", "admin");

		Realm = RealmConfiguration.GetRealm(_keycloak);

		var keycloakRealmBaseUrl = $"http://localhost:{_keycloak.GetMappedPublicPort(KeycloakBuilder.KeycloakPort)}/realms/{Realm.Name}";
		var configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				{ "ConnectionStrings:Gnomeshade", $"Data Source={_databasePath}" },
				{ "Database:Provider", "Sqlite" },
				{ "Oidc:Keycloak:ServerRealm", keycloakRealmBaseUrl },
				{ "Oidc:Keycloak:Metadata", $"{keycloakRealmBaseUrl}/.well-known/openid-configuration" },
				{ "Oidc:Keycloak:ClientId", Client.Name },
				{ "Oidc:Keycloak:ClientSecret", Client.Secret },
				{ "Oidc:Keycloak:RequireHttpsMetadata", "false" },
				{ "OpenTelemetry:Enabled", "false" },
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
