// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Net.Http.Json;
using System.Threading.Tasks;

using Bogus;

using Gnomeshade.Data.Tests.Integration;
using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Authentication;

using Microsoft.AspNetCore.Mvc.Testing;

using NUnit.Framework;

namespace Gnomeshade.Interfaces.WebApi.Tests.Integration
{
	[SetUpFixture]
	public sealed class WebserverSetup
	{
		public static WebApplicationFactory<Startup> WebApplicationFactory { get; private set; } = null!;

		private static Login Login { get; set; } = null!;

		public static IGnomeshadeClient CreateClient()
		{
			var httpClient = WebApplicationFactory.CreateClient();
			return new GnomeshadeClient(httpClient);
		}

		public static async Task<IGnomeshadeClient> CreateAuthorizedClientAsync()
		{
			var client = CreateClient();
			var loginResult = await client.LogInAsync(Login);
			if (loginResult is not SuccessfulLogin)
			{
				throw new(loginResult.ToString());
			}

			return client;
		}

		[OneTimeSetUp]
		public async Task OneTimeSetUpAsync()
		{
			await DatabaseInitialization.SetupDatabaseAsync();
			WebApplicationFactory = new WebApplicationFactory<Startup>().WithWebHostBuilder(builder =>
			{
				builder.UseSetting("ConnectionStrings:FinanceDb", DatabaseInitialization.ConnectionString);
			});

			var registrationModel = new Faker<RegistrationModel>()
				.RuleFor(registration => registration.Email, faker => faker.Internet.Email())
				.RuleFor(registration => registration.Password, faker => faker.Internet.Password(10, 12))
				.RuleFor(registration => registration.Username, faker => faker.Internet.UserName())
				.RuleFor(registration => registration.FullName, faker => faker.Person.FullName)
				.Generate();

			var client = WebApplicationFactory.CreateClient();
			var response = await client.PostAsJsonAsync("api/v1.0/authentication/register", registrationModel);
			response.EnsureSuccessStatusCode();

			Login = new() { Username = registrationModel.Username, Password = registrationModel.Password };
		}

		[OneTimeTearDown]
		public async Task OneTimeTearDownAsync()
		{
			await DatabaseInitialization.DropDatabaseAsync();
		}
	}
}
