// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using FluentAssertions;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Accounts;
using Gnomeshade.Interfaces.WebApi.Models.Products;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;

using Microsoft.AspNetCore.Http;

using NUnit.Framework;

namespace Gnomeshade.Interfaces.WebApi.Tests.Integration.V1_0.Transactions
{
	public class TransactionControllerTests
	{
		private IGnomeshadeClient _client = null!;

		[SetUp]
		public async Task SetUpAsync()
		{
			_client = await WebserverSetup.CreateAuthorizedClientAsync();
		}

		[Test]
		public async Task PutItem()
		{
			var counterparty = await _client.GetMyCounterpartyAsync();

			var currencies = await _client.GetCurrenciesAsync();
			var currency1 = currencies.First();
			var currency2 = currencies.Skip(1).First();

			var account1 = await CreateAccountAsync(currency1, counterparty);
			var account2 = await CreateAccountAsync(currency2, counterparty);

			var productCreation = new ProductCreationModel { Name = Guid.NewGuid().ToString("N") };
			var productId = await _client.PutProductAsync(productCreation);

			var itemCreationModel = new TransactionItemCreationModel
			{
				SourceAmount = 1,
				SourceAccountId = account1.Currencies.Single().Id,
				TargetAmount = 2,
				TargetAccountId = account2.Currencies.Single().Id,
				ProductId = productId,
				Amount = 1,
			};

			var transactionCreationModel = new TransactionCreationModel
			{
				Date = DateTimeOffset.Now,
				Items = new() { itemCreationModel },
			};

			var transactionId = await _client.CreateTransactionAsync(transactionCreationModel);
			var transaction = await _client.GetTransactionAsync(transactionId);
			var createdItem = transaction.Items.Should().ContainSingle().Subject;

			var unchangedItemCreationModel = itemCreationModel with { Id = createdItem.Id };
			_ = await _client.PutTransactionItemAsync(transactionId, unchangedItemCreationModel);
			var unchangedTransaction = await _client.GetTransactionAsync(transactionId);
			var unchangedItem = unchangedTransaction.Items.Should().ContainSingle().Subject;

			unchangedItem.Should().BeEquivalentTo(createdItem, options => options.ComparingByMembers<TransactionItemModel>().Excluding(item => item.ModifiedAt));
			unchangedItem.ModifiedAt.Should().BeAfter(createdItem.ModifiedAt);

			_ = await _client.PutTransactionItemAsync(transactionId, itemCreationModel);
			var transactionWithAdditionalItem = await _client.GetTransactionAsync(transactionId);
			transactionWithAdditionalItem.Items.Should().HaveCount(2);

			FluentActions
				.Awaiting(() => _client.PutTransactionItemAsync(Guid.NewGuid(), itemCreationModel))
				.Should()
				.ThrowExactly<HttpRequestException>()
				.Which.StatusCode.Should().Be(StatusCodes.Status404NotFound);
		}

		private async Task<Account> CreateAccountAsync(Currency currency, Counterparty counterparty)
		{
			var creationModel = new AccountCreationModel
			{
				Name = Guid.NewGuid().ToString("N"),
				CounterpartyId = counterparty.Id,
				PreferredCurrencyId = currency.Id,
				Currencies = new() { new() { CurrencyId = currency.Id } },
			};
			var accountId = await _client.CreateAccountAsync(creationModel);
			return await _client.GetAccountAsync(accountId);
		}
	}
}
