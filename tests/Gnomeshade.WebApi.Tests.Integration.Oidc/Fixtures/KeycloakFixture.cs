// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;

using Microsoft.Extensions.Configuration;

using NodaTime;

using TUnit.Core.Interfaces;

using VMelnalksnis.Testcontainers.Keycloak;
using VMelnalksnis.Testcontainers.Keycloak.Configuration;

using KeycloakClient = VMelnalksnis.Testcontainers.Keycloak.Configuration.Client;

namespace Gnomeshade.WebApi.Tests.Integration.Oidc.Fixtures;

public sealed class KeycloakFixture : IAsyncInitializer, IAsyncDisposable
{
	private readonly int _port1;

	private readonly string _databasePath;
	private readonly KeycloakContainer _keycloak;
	private WebApplicationFactory _application = null!;

	public KeycloakFixture(int port1, int port2)
	{
		_port1 = port1;
		DesktopBaseUri = $"http://localhost:{port2}/";
		_databasePath = $"gnomeshade_{Guid.NewGuid()}.db";
		var apiBaseUri = $"https://localhost:{port1}/";

		File.Delete(_databasePath);

		var mapper = new ClientProtocolMapper("audience-mapping", "openid-connect", "oidc-audience-mapper")
		{
			IncludedClientAudience = "gnomeshade",
		};

		Client = new("gnomeshade")
		{
			Mappers = [mapper],
			Secret = Guid.NewGuid().ToString(),
			RedirectUris = [$"{apiBaseUri}*"],
		};

		DesktopClient = new("gnomeshade_desktop")
		{
			Mappers = [mapper],
			Secret = Guid.NewGuid().ToString(),
			RedirectUris = [DesktopBaseUri],
		};

		var realmConfiguration = new RealmConfiguration(
			"demorealm",
			new List<KeycloakClient> { Client, DesktopClient },
			new List<User> { User });

		_keycloak = new KeycloakBuilder().WithRealm(realmConfiguration).Build();
	}

	public string DesktopBaseUri { get; }

	public string Name => $"{_port1}";

	internal Realm Realm { get; private set; } = null!;

	internal KeycloakClient Client { get; }

	internal KeycloakClient DesktopClient { get; }

	internal User User { get; } = new("john.doe", "password123")
	{
		Email = "john.doe@example.com",
		EmailVerified = true,
		FirstName = "John",
		LastName = "Doe",
	};

	/// <inheritdoc />
	public async Task InitializeAsync()
	{
		await _keycloak.StartAsync();

		Realm = _keycloak.Realm;
	}

	public void PreFirstTest()
	{
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
			})
			.AddEnvironmentVariables()
			.Build();

		_application = new(configuration, _port1);
		_application.CreateClient();
	}

	/// <inheritdoc />
	public ValueTask DisposeAsync()
	{
		File.Delete(_databasePath);
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
