// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using FluentAssertions;
using FluentAssertions.Equivalency;

using Gnomeshade.Data.Entities.Abstractions;
using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Accounts;
using Gnomeshade.Interfaces.WebApi.Models.Products;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;
using Gnomeshade.TestingHelpers.Models;

using NUnit.Framework;

namespace Gnomeshade.Interfaces.WebApi.Tests.Integration.V1_0.Transactions
{
	public class TransactionControllerTests
	{
		private IGnomeshadeClient _client = null!;

		private Counterparty _counterparty = null!;
		private Account _account1 = null!;
		private Account _account2 = null!;
		private Guid _productId;
		private TransactionItemCreationFaker _itemCreationFaker = null!;

		[SetUp]
		public async Task SetUpAsync()
		{
			_client = await WebserverSetup.CreateAuthorizedClientAsync();

			_counterparty = await _client.GetMyCounterpartyAsync();
			var currencies = await _client.GetCurrenciesAsync();
			var currency1 = currencies.First();
			var currency2 = currencies.Skip(1).First();

			_account1 = await CreateAccountAsync(currency1, _counterparty);
			_account2 = await CreateAccountAsync(currency2, _counterparty);

			var productCreation = new ProductCreationModel { Name = Guid.NewGuid().ToString("N") };
			_productId = await _client.PutProductAsync(productCreation);

			var sourceAccountId = _account1.Currencies.Single().Id;
			var targetAccountId = _account2.Currencies.Single().Id;
			_itemCreationFaker = new(sourceAccountId, targetAccountId, _productId);
		}

		[Test]
		public async Task PutItem()
		{
			var itemCreationModel = _itemCreationFaker.Generate();

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

			unchangedItem.Should().BeEquivalentTo(createdItem, WithoutModifiedAt);
			unchangedItem.ModifiedAt.Should().BeAfter(createdItem.ModifiedAt);

			_ = await _client.PutTransactionItemAsync(transactionId, itemCreationModel);
			var transactionWithAdditionalItem = await _client.GetTransactionAsync(transactionId);
			transactionWithAdditionalItem.Items.Should().HaveCount(2);

			var exception =
				await FluentActions
					.Awaiting(() => _client.PutTransactionItemAsync(Guid.NewGuid(), itemCreationModel))
					.Should()
					.ThrowExactlyAsync<HttpRequestException>();
			exception.Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
		}

		[Test]
		public async Task PutTransaction()
		{
			var items = _itemCreationFaker.Generate(2);
			var transactionCreationModel = new TransactionCreationModel
			{
				Date = DateTimeOffset.Now,
				Items = items,
			};

			var transactionId = await _client.PutTransactionAsync(transactionCreationModel);
			var transaction = await _client.GetTransactionAsync(transactionId);

			transactionCreationModel = transactionCreationModel with
			{
				Id = transaction.Id,
				Items = transactionCreationModel.Items
					.Zip(transaction.Items)
					.Select(tuple => tuple.First with { Id = tuple.Second.Id })
					.ToList(),
			};

			var identicalId = await _client.PutTransactionAsync(transactionCreationModel);
			var identicalTransaction = await _client.GetTransactionAsync(identicalId);

			transaction.Should().BeEquivalentTo(identicalTransaction, WithoutModifiedAt);
		}

		private static EquivalencyAssertionOptions<Transaction> WithoutModifiedAt(
			EquivalencyAssertionOptions<Transaction> options)
		{
			return options
				.ComparingByMembers<Transaction>()
				.ComparingByMembers<TransactionItem>()
				.ComparingByMembers<Product>()
				.Excluding(info => info.Name == nameof(IModifiableEntity.ModifiedAt));
		}

		private static EquivalencyAssertionOptions<TransactionItem> WithoutModifiedAt(
			EquivalencyAssertionOptions<TransactionItem> options)
		{
			return options
				.ComparingByMembers<TransactionItem>()
				.Excluding(info => info.Name == nameof(IModifiableEntity.ModifiedAt));
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
