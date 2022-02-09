// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NUnit.Framework;

using VMelnalksnis.OAuth2.Keycloak;

namespace VMelnalksnis.OAuth2.Tests.Integration;

[SetUpFixture]
public class TestConfiguration
{
	private static IServiceProvider ServiceProvider { get; set; } = null!;

	[OneTimeSetUp]
	public static void OneTimeSetUp()
	{
		var configuration = new ConfigurationBuilder()
			.AddUserSecrets<TestConfiguration>()
			.Build();

		var serviceCollection = new ServiceCollection();
		serviceCollection
			.AddOptions<KeycloakOAuth2ClientOptions>()
			.Bind(configuration.GetSection("Oidc:Keycloak"))
			.ValidateDataAnnotations();

		serviceCollection.AddLogging(builder => builder.AddConsole());
		serviceCollection.AddHttpClient<KeycloakOAuth2Client>();
		serviceCollection.AddScoped<KeycloakOAuth2Client>();

		ServiceProvider = serviceCollection.BuildServiceProvider();
	}

	public static TService GetRequiredService<TService>()
		where TService : notnull
	{
		return ServiceProvider.GetRequiredService<TService>();
	}
}
