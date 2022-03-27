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
using Gnomeshade.Interfaces.WebApi.V1_0.Transactions;
using Gnomeshade.TestingHelpers.Models;

using NUnit.Framework;

namespace Gnomeshade.Interfaces.WebApi.Tests.Integration.V1_0.Transactions;

[TestOf(typeof(TransactionsController))]
public class TransactionsControllerTests
{
	private IGnomeshadeClient _client = null!;
	private IGnomeshadeClient _secondClient = null!;

	private Counterparty _counterparty = null!;
	private Account _account1 = null!;
	private Account _account2 = null!;
	private Guid _productId;
	private TransactionItemCreationFaker _itemCreationFaker = null!;

	[SetUp]
	public async Task SetUpAsync()
	{
		_client = await WebserverSetup.CreateAuthorizedClientAsync();
		_secondClient = await WebserverSetup.CreateAuthorizedSecondClientAsync();

		_counterparty = await _client.GetMyCounterpartyAsync();
		var currencies = await _client.GetCurrenciesAsync();
		var currency1 = currencies.First();
		var currency2 = currencies.Skip(1).First();

		_account1 = await CreateAccountAsync(currency1, _counterparty);
		_account2 = await CreateAccountAsync(currency2, _counterparty);

		var productCreation = new ProductCreationModel { Name = Guid.NewGuid().ToString("N") };
		_productId = Guid.NewGuid();
		await _client.PutProductAsync(_productId, productCreation);

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
			ValuedAt = DateTimeOffset.Now,
			Items = new() { itemCreationModel },
		};

		var transactionId = await _client.CreateTransactionAsync(transactionCreationModel);
		var transaction = await _client.GetTransactionAsync(transactionId);
		var createdItem = transaction.Items.Should().ContainSingle().Subject;

		await _client.PutTransactionItemAsync(createdItem.Id, transactionId, itemCreationModel);
		var unchangedTransaction = await _client.GetTransactionAsync(transactionId);
		var unchangedItem = unchangedTransaction.Items.Should().ContainSingle().Subject;

		unchangedItem.Should().BeEquivalentTo(createdItem, WithoutModifiedAt);
		unchangedItem.ModifiedAt.Should().BeAfter(createdItem.ModifiedAt);

		await _client.PutTransactionItemAsync(Guid.NewGuid(), transactionId, itemCreationModel);
		var transactionWithAdditionalItem = await _client.GetTransactionAsync(transactionId);
		transactionWithAdditionalItem.Items.Should().HaveCount(2);

		var exception =
			await FluentActions
				.Awaiting(() => _client.PutTransactionItemAsync(createdItem.Id, Guid.NewGuid(), itemCreationModel))
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
			ValuedAt = DateTimeOffset.Now,
			Items = items,
		};

		var transactionId = Guid.NewGuid();
		await _client.PutTransactionAsync(transactionId, transactionCreationModel);
		var transaction = await _client.GetTransactionAsync(transactionId);

		transactionCreationModel = transactionCreationModel with
		{
			Items = transactionCreationModel.Items
				.Zip(transaction.Items)
				.Select(tuple => tuple.First)
				.ToList(),
		};

		await _client.PutTransactionAsync(transactionId, transactionCreationModel);
		var identicalTransaction = await _client.GetTransactionAsync(transactionId);

		transaction.Should().BeEquivalentTo(identicalTransaction, WithoutModifiedAt);
	}

	[Test]
	public async Task GetTransaction_ShouldReturnNotFoundForOtherUsers()
	{
		var transactionCreationModel = new TransactionCreationModel
		{
			ValuedAt = DateTimeOffset.Now,
			Items = new() { _itemCreationFaker.Generate() },
		};

		var transactionId = await _client.CreateTransactionAsync(transactionCreationModel);

		await FluentActions
			.Awaiting(async () => await _client.GetTransactionAsync(transactionId))
			.Should()
			.NotThrowAsync();

		(await FluentActions
				.Awaiting(async () => await _secondClient.GetTransactionAsync(transactionId))
				.Should()
				.ThrowAsync<HttpRequestException>())
			.Which.StatusCode.Should()
			.Be(HttpStatusCode.NotFound);
	}

	[Test]
	public async Task TagUntagTransactionItem()
	{
		var transactionCreationModel = new TransactionCreationModel
		{
			ValuedAt = DateTimeOffset.Now,
			Items = new() { _itemCreationFaker.Generate() },
		};

		var transactionId = await _client.CreateTransactionAsync(transactionCreationModel);
		var transaction = await _client.GetTransactionAsync(transactionId);
		var itemId = transaction.Items.First().Id;

		var tagId = Guid.NewGuid();
		await _client.PutTagAsync(tagId, new() { Name = $"{tagId:N}" });

		await _client.TagTransactionItemAsync(itemId, tagId);
		(await _client.GetTransactionItemTagsAsync(itemId))
			.Should()
			.ContainSingle()
			.Which.Id.Should()
			.Be(tagId);

		await _client.UntagTransactionItemAsync(itemId, tagId);
		(await _client.GetTransactionItemTagsAsync(itemId))
			.Should()
			.BeEmpty();
	}

	[Test]
	public async Task Transfers()
	{
		var transactionCreationModel = new TransactionCreationModel
		{
			ValuedAt = DateTimeOffset.Now,
			Items = new() { _itemCreationFaker.Generate() },
		};

		var transactionId = await _client.CreateTransactionAsync(transactionCreationModel);

		var transferId = Guid.NewGuid();
		var transferCreation = new TransferCreation
		{
			SourceAmount = 1,
			SourceAccountId = _account1.Currencies.First().Id,
			TargetAmount = 1,
			TargetAccountId = _account2.Currencies.First().Id,
		};

		await _client.PutTransferAsync(transactionId, transferId, transferCreation);
		var transfer = await _client.GetTransferAsync(transactionId, transferId);
		var transfers = await _client.GetTransfersAsync(transactionId);

		transfers.Should().ContainSingle().Which.Should().BeEquivalentTo(transfer);
		transfer.Should().BeEquivalentTo(transferCreation);

		var bankReference = $"{Guid.NewGuid():N}";
		transferCreation = transferCreation with { BankReference = bankReference };
		await _client.PutTransferAsync(transactionId, transferId, transferCreation);
		(await _client.GetTransferAsync(transactionId, transferId)).BankReference.Should().Be(bankReference);

		await _client.DeleteTransferAsync(transactionId, transferId);
		(await _client.GetTransfersAsync(transactionId)).Should().BeEmpty();
		(await FluentActions
				.Awaiting(() => _client.GetTransferAsync(transactionId, transferId))
				.Should()
				.ThrowExactlyAsync<HttpRequestException>())
			.Which.StatusCode.Should()
			.Be(HttpStatusCode.NotFound);
	}

	[Test]
	public async Task PutTransfer_NonExistentTransaction()
	{
		var transactionId = Guid.NewGuid();
		var transferId = Guid.NewGuid();
		var transferCreation = new TransferCreation
		{
			SourceAmount = 1,
			SourceAccountId = _account1.Currencies.First().Id,
			TargetAmount = 1,
			TargetAccountId = _account2.Currencies.First().Id,
		};

		(await FluentActions
				.Awaiting(() => _client.PutTransferAsync(transactionId, transferId, transferCreation))
				.Should()
				.ThrowExactlyAsync<HttpRequestException>())
			.Which.StatusCode.Should()
			.Be(HttpStatusCode.NotFound);
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
