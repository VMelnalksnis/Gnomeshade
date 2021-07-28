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

using Gnomeshade.Interfaces.WebApi.V1_0.Accounts;
using Gnomeshade.Interfaces.WebApi.V1_0.Products;
using Gnomeshade.Interfaces.WebApi.V1_0.Transactions;

using NUnit.Framework;

namespace Gnomeshade.Interfaces.WebApi.Tests.Integration
{
	public class BasicTests
	{
		private HttpClient _authenticatedClient = null!;

		[SetUp]
		public async Task SetUpAsync()
		{
			_authenticatedClient = await WebserverSetup.CreateAuthorizedClientAsync();
		}

		[TestCase("v1.0")]
		public async Task Get_SwaggerEndpoint(string version)
		{
			var response = await _authenticatedClient.GetAsync($"/swagger/{version}/swagger.json");

			response.EnsureSuccessStatusCode();
			_ = await response.Content.ReadAsStringAsync();
		}

		[TestCase("/swagger")]
		public async Task Get_ReturnsSuccess(string requestUrl)
		{
			var response = await _authenticatedClient.GetAsync(requestUrl);

			response.EnsureSuccessStatusCode();
			_ = await response.Content.ReadAsStringAsync();
		}

		[Test]
		public async Task Get_ShouldReturnUnauthorized()
		{
			var client = WebserverSetup.WebApplicationFactory.CreateClient();

			var response = await client.GetAsync("/api/v1.0/transaction");

			response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
		}

		[Test]
		public async Task Login()
		{
			var authorizedResponse = await _authenticatedClient.GetAsync("/api/v1.0/transaction");
			authorizedResponse.EnsureSuccessStatusCode();
		}

		[Test]
		public async Task Create_ShouldCreateItems()
		{
			var currency = (await _authenticatedClient.GetFromJsonAsync<List<CurrencyModel>>("/api/v1.0/currency"))!
				.First();
			var productId = (await _authenticatedClient.GetFromJsonAsync<List<ProductModel>>("/api/v1.0/product"))!
				.FirstOrDefault()
				?.Id;
			if (productId is null)
			{
				var productCreationModel = new Faker<ProductCreationModel>()
					.RuleFor(model => model.Name, faker => faker.Commerce.ProductName())
					.RuleFor(model => model.Description, faker => faker.Lorem.Sentence())
					.Generate();

				var productCreationResponse =
					await _authenticatedClient.PostAsJsonAsync("/api/v1.0/product", productCreationModel);
				productCreationResponse.EnsureSuccessStatusCode();
				productId = await productCreationResponse.Content.ReadFromJsonAsync<Guid>();
			}

			var accountCreationModel =
				new Faker<AccountCreationModel>()
					.RuleFor(model => model.Name, faker => faker.Finance.AccountName())
					.RuleFor(model => model.Bic, faker => faker.Finance.Bic())
					.RuleFor(model => model.Iban, faker => faker.Finance.Iban())
					.RuleFor(model => model.PreferredCurrencyId, () => currency.Id)
					.RuleFor(model => model.Currencies, () => new() { new() { CurrencyId = currency.Id } })
					.Generate();

			var findAccountResponse =
				await _authenticatedClient.GetAsync($"/api/v1.0/account/find/{accountCreationModel.Name}");

			var account =
				findAccountResponse.IsSuccessStatusCode
					? await findAccountResponse.Content.ReadFromJsonAsync<AccountModel>()
					: null;

			if (account is null)
			{
				var accountCreationResponse =
					await _authenticatedClient.PostAsJsonAsync("/api/v1.0/account", accountCreationModel);
				accountCreationResponse.EnsureSuccessStatusCode();
				var accountId = await accountCreationResponse.Content.ReadFromJsonAsync<Guid>();
				account =
					(await _authenticatedClient.GetFromJsonAsync<AccountModel>($"/api/v1.0/account/{accountId:N}"))!;
			}

			var transactionsResponse = await _authenticatedClient.GetAsync("/api/v1.0/transaction");
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
						ProductId = productId,
						Amount = 0,
					},
				},
			};

			var createResponse = await _authenticatedClient.PostAsJsonAsync("/api/v1.0/transaction", transaction);
			createResponse.EnsureSuccessStatusCode();
		}
	}
}
