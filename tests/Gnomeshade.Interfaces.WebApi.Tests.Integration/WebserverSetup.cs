// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

using Bogus;

using Gnomeshade.Data.Tests.Integration;
using Gnomeshade.Interfaces.WebApi.V1_0.Authentication;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;

using NUnit.Framework;

namespace Gnomeshade.Interfaces.WebApi.Tests.Integration
{
	[SetUpFixture]
	public sealed class WebserverSetup
	{
		public static WebApplicationFactory<Startup> WebApplicationFactory { get; private set; } = null!;

		private static LoginModel LoginModel { get; set; } = null!;

		public static async Task<HttpClient> CreateAuthorizedClientAsync()
		{
			var client = WebApplicationFactory.CreateClient();
			var loginResponse = await client.PostAsJsonAsync("/api/v1.0/authentication/login", LoginModel);
			loginResponse.EnsureSuccessStatusCode();
			var responseContent = (await loginResponse.Content.ReadFromJsonAsync<LoginResponse>())!;

			client.DefaultRequestHeaders.Authorization = new(JwtBearerDefaults.AuthenticationScheme, responseContent.Token);
			return client;
		}

		[OneTimeSetUp]
		public async Task OneTimeSetUpAsync()
		{
			await DatabaseInitialization.SetupDatabaseAsync();
			WebApplicationFactory = new();

			var registrationModel = new Faker<RegistrationModel>()
				.RuleFor(registration => registration.Email, faker => faker.Internet.Email())
				.RuleFor(registration => registration.Password, faker => faker.Internet.Password(10, 12))
				.RuleFor(registration => registration.Username, faker => faker.Internet.UserName())
				.Generate();

			var client = WebApplicationFactory.CreateClient();
			var response = await client.PostAsJsonAsync("api/v1.0/authentication/register", registrationModel);
			response.EnsureSuccessStatusCode();

			LoginModel = new() { Username = registrationModel.Username, Password = registrationModel.Password };
		}

		[OneTimeTearDown]
		public async Task OneTimeTearDownAsync()
		{
			await DatabaseInitialization.DropDatabaseAsync();
		}
	}
}
