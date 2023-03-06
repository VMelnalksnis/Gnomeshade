// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

using Bogus;

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
	private WebApplicationFactory<Startup> _webApplicationFactory = null!;

	internal abstract string Name { get; }

	protected abstract IEnumerable<ITestcontainersContainer> Containers { get; }

	public async ValueTask DisposeAsync()
	{
		await _webApplicationFactory.DisposeAsync();
		await Task.WhenAll(Containers.Select(container => container.StopAsync()));
	}

	internal async Task Initialize()
	{
		await Task.WhenAll(Containers.Select(container => container.StartAsync()));

		var configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				{ "Paperless:BaseAddress", "https://localhost/" },
				{ "Paperless:Token", "not-used" },
			})
			.AddConfiguration(GetAdditionalConfiguration())
			.AddEnvironmentVariables()
			.Build();

		_webApplicationFactory = new GnomeshadeWebApplicationFactory(configuration);
		_webApplicationFactory.CreateClient();
	}

	internal HttpClient CreateHttpClient(params DelegatingHandler[] handlers) =>
		_webApplicationFactory.CreateDefaultClient(handlers);

	internal GnomeshadeClient CreateUnauthorizedClient(params DelegatingHandler[] handlers)
	{
		var httpClient = CreateHttpClient(handlers);
		return new(httpClient, new(DateTimeZoneProviders.Tzdb));
	}

	internal async Task<IGnomeshadeClient> CreateAuthorizedClientAsync()
	{
		var client = _webApplicationFactory.CreateClient();
		var registrationFaker = new Faker<RegistrationModel>()
			.RuleFor(registration => registration.Email, faker => faker.Internet.Email())
			.RuleFor(registration => registration.Password, faker => faker.Internet.Password(10, 12))
			.RuleFor(registration => registration.Username, faker => faker.Internet.UserName())
			.RuleFor(registration => registration.FullName, faker => faker.Person.FullName);

		var login = await RegisterUser(client, registrationFaker.Generate());
		return await CreateAuthorizedClientAsync(login);
	}

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
