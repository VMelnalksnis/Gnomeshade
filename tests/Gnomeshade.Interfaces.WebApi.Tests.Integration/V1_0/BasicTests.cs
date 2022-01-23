// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Bogus;

using FluentAssertions;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Accounts;
using Gnomeshade.Interfaces.WebApi.Models.Products;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;

using NUnit.Framework;

namespace Gnomeshade.Interfaces.WebApi.Tests.Integration.V1_0;

public class BasicTests
{
	private IGnomeshadeClient _authenticatedClient = null!;

	[SetUp]
	public async Task SetUpAsync()
	{
		_authenticatedClient = await WebserverSetup.CreateAuthorizedClientAsync();
	}

	[Test]
	public async Task Get_ShouldReturnUnauthorized()
	{
		var client = WebserverSetup.CreateHttpClient();

		var response = await client.GetAsync("/api/v1.0/transaction");

		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
	}

	[Test]
	public async Task Create_ShouldCreateItems()
	{
		var currency = (await _authenticatedClient.GetCurrenciesAsync()).First();
		var productId = (await _authenticatedClient.GetProductsAsync()).FirstOrDefault()?.Id;
		if (productId is null)
		{
			var productCreationModel = new Faker<ProductCreationModel>()
				.RuleFor(model => model.Name, faker => faker.Commerce.ProductName())
				.RuleFor(model => model.Description, faker => faker.Lorem.Sentence())
				.Generate();

			productId = Guid.NewGuid();
			await _authenticatedClient.PutProductAsync(productId.Value, productCreationModel);
		}

		var counterparty = await _authenticatedClient.GetMyCounterpartyAsync();
		var accountCreationModel =
			new Faker<AccountCreationModel>()
				.RuleFor(model => model.Name, faker => faker.Finance.AccountName())
				.RuleFor(model => model.Bic, faker => faker.Finance.Bic())
				.RuleFor(model => model.Iban, faker => faker.Finance.Iban())
				.RuleFor(model => model.CounterpartyId, counterparty.Id)
				.RuleFor(model => model.PreferredCurrencyId, currency.Id)
				.RuleFor(model => model.Currencies, () => new() { new() { CurrencyId = currency.Id } })
				.Generate();

		var account = await _authenticatedClient.FindAccountAsync(accountCreationModel.Name!);
		if (account is null)
		{
			var accountId = await _authenticatedClient.CreateAccountAsync(accountCreationModel);
			account = await _authenticatedClient.GetAccountAsync(accountId);
		}

		await _authenticatedClient.GetTransactionsAsync(null, null);

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

		var transactionId = await _authenticatedClient.CreateTransactionAsync(transaction);
		await _authenticatedClient.GetTransactionAsync(transactionId);
	}
}
