// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

using Bogus;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Authentication;
using Gnomeshade.TestingHelpers;
using Gnomeshade.TestingHelpers.Data;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

using NUnit.Framework;

namespace Gnomeshade.Interfaces.WebApi.Tests.Integration
{
	[SetUpFixture]
	public sealed class WebserverSetup
	{
		private static readonly PostgresInitializer _initializer;

		private static WebApplicationFactory<Startup> _webApplicationFactory = null!;
		private static Login _login = null!;

		static WebserverSetup()
		{
			IConfiguration configuration =
				new ConfigurationBuilder()
					.AddUserSecrets<WebserverSetup>()
					.AddEnvironmentVariables()
					.Build();

			_initializer = new(configuration);
		}

		public static HttpClient CreateHttpClient() => _webApplicationFactory.CreateClient();

		public static GnomeshadeClient CreateUnauthorizedClient() => new(CreateHttpClient());

		public static async Task<IGnomeshadeClient> CreateAuthorizedClientAsync()
		{
			var client = CreateUnauthorizedClient();
			var loginResult = await client.LogInAsync(_login);
			if (loginResult is not SuccessfulLogin)
			{
				throw new(loginResult.ToString());
			}

			return client;
		}

		[OneTimeSetUp]
		public async Task OneTimeSetUpAsync()
		{
			_webApplicationFactory = new WebApplicationFactory<Startup>().WithWebHostBuilder(builder =>
			{
				builder.ConfigureAppConfiguration((_, configurationBuilder) =>
				{
					configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
					{
						{ "ConnectionStrings:FinanceDb", _initializer.ConnectionString },
					});
				});
			});
			var client = _webApplicationFactory.CreateClient();

			// database needs to be setup after web app, so that the web app can configure static Npg logger
			await _initializer.SetupDatabaseAsync();

			var registrationModel = new Faker<RegistrationModel>()
				.RuleFor(registration => registration.Email, faker => faker.Internet.Email())
				.RuleFor(registration => registration.Password, faker => faker.Internet.Password(10, 12))
				.RuleFor(registration => registration.Username, faker => faker.Internet.UserName())
				.RuleFor(registration => registration.FullName, faker => faker.Person.FullName)
				.Generate();

			var response = await client.PostAsJsonAsync("api/v1.0/authentication/register", registrationModel);
			response.EnsureSuccessStatusCode();

			_login = new() { Username = registrationModel.Username, Password = registrationModel.Password };
		}

		[OneTimeTearDown]
		public async Task OneTimeTearDownAsync()
		{
			await _initializer.DropDatabaseAsync();
		}
	}
}
