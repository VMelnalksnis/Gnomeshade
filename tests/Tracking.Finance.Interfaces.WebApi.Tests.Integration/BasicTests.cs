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

using FluentAssertions;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

using NUnit.Framework;

using Tracking.Finance.Interfaces.WebApi.Configuration;
using Tracking.Finance.Interfaces.WebApi.V1_0.Accounts;
using Tracking.Finance.Interfaces.WebApi.V1_0.Authentication;
using Tracking.Finance.Interfaces.WebApi.V1_0.Transactions;

namespace Tracking.Finance.Interfaces.WebApi.Tests.Integration
{
	public class BasicTests
	{
		private WebApplicationFactory<Startup> _applicationFactory = null!;
		private AuthenticatedUserOptions _userOptions = null!;

		private HttpClient _client = null!;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_applicationFactory = new();

			var builder = new ConfigurationBuilder()
				.AddUserSecrets<BasicTests>()
				.AddEnvironmentVariables();

			var configuration = builder.Build();
			_userOptions = configuration.GetValid<AuthenticatedUserOptions>();
		}

		[SetUp]
		public void SetUp()
		{
			_client = _applicationFactory.CreateClient();
		}

		[TestCase("v1.0")]
		public async Task Get_SwaggerEndpoint(string version)
		{
			var response = await _client.GetAsync($"/swagger/{version}/swagger.json");

			response.EnsureSuccessStatusCode();
			_ = await response.Content.ReadAsStringAsync();
		}

		[TestCase("/swagger")]
		public async Task Get_ReturnsSuccess(string requestUrl)
		{
			var response = await _client.GetAsync(requestUrl);

			response.EnsureSuccessStatusCode();
			_ = await response.Content.ReadAsStringAsync();
		}

		[Test]
		public async Task Get_ShouldReturnUnauthorized()
		{
			var response = await _client.GetAsync("/api/v1.0/transaction");

			response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
		}

		[Test]
		public async Task Register()
		{
			var registrationModel = new Faker<RegistrationModel>()
				.RuleFor(registration => registration.Email, faker => faker.Internet.Email())
				.RuleFor(registration => registration.Password, faker => faker.Internet.Password(10, 12))
				.RuleFor(registration => registration.Username, faker => faker.Internet.UserName())
				.Generate();

			var response = await _client.PostAsJsonAsync("api/v1.0/authentication/register", registrationModel);
			response.EnsureSuccessStatusCode();

			var loginModel = new LoginModel
				{ Username = registrationModel.Username, Password = registrationModel.Password };
			var loginResponse = await _client.PostAsJsonAsync("/api/v1.0/authentication/login", loginModel);
			loginResponse.EnsureSuccessStatusCode();
		}

		[Test]
		public async Task Login()
		{
			await AuthorizeAsync();

			var authorizedResponse = await _client.GetAsync("/api/v1.0/transaction");
			authorizedResponse.EnsureSuccessStatusCode();
		}

		[Test]
		public async Task Create_ShouldCreateItems()
		{
			await AuthorizeAsync();
			var currency = (await _client.GetFromJsonAsync<List<CurrencyModel>>("/api/v1.0/currency"))!.First();

			var accountCreationModel =
				new Faker<AccountCreationModel>()
					.RuleFor(model => model.Name, faker => faker.Finance.AccountName())
					.RuleFor(model => model.Bic, faker => faker.Finance.Bic())
					.RuleFor(model => model.Iban, faker => faker.Finance.Iban())
					.RuleFor(model => model.PreferredCurrencyId, () => currency.Id)
					.RuleFor(model => model.Currencies, () => new() { new() { CurrencyId = currency.Id } })
					.Generate();

			var findAccountResponse = await _client.GetAsync($"/api/v1.0/account/find/{accountCreationModel.Name}");

			var account =
				findAccountResponse.IsSuccessStatusCode
					? await findAccountResponse.Content.ReadFromJsonAsync<AccountModel>()
					: null;

			if (account is null)
			{
				var accountCreationResponse = await _client.PostAsJsonAsync("/api/v1.0/account", accountCreationModel);
				accountCreationResponse.EnsureSuccessStatusCode();
				var accountId = await accountCreationResponse.Content.ReadFromJsonAsync<Guid>();
				account = (await _client.GetFromJsonAsync<AccountModel>($"/api/v1.0/account/{accountId:N}"))!;
			}

			var transactionsResponse = await _client.GetAsync("/api/v1.0/transaction");
			transactionsResponse.EnsureSuccessStatusCode();
			await transactionsResponse.Content.ReadFromJsonAsync<List<TransactionModel>>();

			var transaction = new TransactionCreationModel
			{
				Date = DateTimeOffset.Now,
				Description = "Transaction with items creation test",
				Items = new()
				{
					new()
					{
						SourceAccountId = account.Currencies.First().Id,
						TargetAccountId = account.Currencies.First().Id,
						SourceAmount = 0,
						TargetAmount = 0,
						ProductId = Guid.Empty,
						Amount = 0,
					},
				},
			};

			var createResponse = await _client.PostAsJsonAsync("/api/v1.0/transaction", transaction);
			createResponse.EnsureSuccessStatusCode();
		}

		private async Task AuthorizeAsync()
		{
			var login = new LoginModel { Username = _userOptions.Username, Password = _userOptions.Password };

			var loginResponse =
				await _client.PostAsJsonAsync("/api/v1.0/authentication/login", login).ConfigureAwait(false);
			loginResponse.EnsureSuccessStatusCode();
			var responseContent =
				(await loginResponse.Content.ReadFromJsonAsync<LoginResponse>().ConfigureAwait(false))!;

			_client.DefaultRequestHeaders.Authorization =
				new(JwtBearerDefaults.AuthenticationScheme, responseContent.Token);
		}
	}
}
