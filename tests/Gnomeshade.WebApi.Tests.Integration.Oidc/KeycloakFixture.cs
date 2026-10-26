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

namespace Gnomeshade.WebApi.Tests.Integration.Oidc;

public sealed class KeycloakFixture : IAsyncDisposable
{
	internal const string ApiBaseUri = "https://localhost:5001/";
	internal const string DesktopBaseUri = "http://localhost:8297/";
	internal const string ClientId = "gnomeshade";
	internal const string DesktopClientId = "gnomeshade_desktop";
	internal const string Username = "john.doe";
	internal const string Password = "password123";
	private const string _realmName = "demorealm";
	private const string _databasePath = "gnomeshade.db";

	private static int _keycloakPublicPort = 45278;

	private readonly KeycloakContainer _keycloak;
	private GnomeshadeWebApplicationFactory _application = null!;

	public KeycloakFixture()
	{
		File.Delete(_databasePath);

		var publicPort = Interlocked.Increment(ref _keycloakPublicPort);

		_keycloak = new KeycloakBuilder("quay.io/keycloak/keycloak:26.8.1")
			.WithPortBinding(publicPort, KeycloakBuilder.KeycloakPort)
			.WithEnvironment("KC_HOSTNAME", $"http://localhost:{publicPort}")
			.WithEnvironment("KC_HOSTNAME_BACKCHANNEL_DYNAMIC", "true")
			.WithEnvironment("GNOMESHADE_TEST_CLIENT_SECRET", ClientSecret)
			.WithEnvironment("GNOMESHADE_TEST_DESKTOP_CLIENT_SECRET", DesktopClientSecret)
			.WithRealm(Path.Combine(AppContext.BaseDirectory, "realm.json"))
			.Build();
	}

	internal string ClientSecret { get; } = Guid.NewGuid().ToString();

	internal string DesktopClientSecret { get; } = Guid.NewGuid().ToString();

	internal string RealmBaseUrl { get; private set; } = null!;

	public async Task Initialize()
	{
		await _keycloak.StartAsync();

		RealmBaseUrl = $"http://localhost:{_keycloak.GetMappedPublicPort(KeycloakBuilder.KeycloakPort)}/realms/{_realmName}";
		var configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				{ "ConnectionStrings:Gnomeshade", $"Data Source={_databasePath}" },
				{ "Database:Provider", "Sqlite" },
				{ "Oidc:Keycloak:ServerRealm", RealmBaseUrl },
				{ "Oidc:Keycloak:Metadata", $"{RealmBaseUrl}/.well-known/openid-configuration" },
				{ "Oidc:Keycloak:ClientId", ClientId },
				{ "Oidc:Keycloak:ClientSecret", ClientSecret },
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
