﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Gnomeshade.Data.Entities.Abstractions;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models;
using Gnomeshade.WebApi.Models.Accounts;
using Gnomeshade.WebApi.Models.Products;
using Gnomeshade.WebApi.Models.Transactions;
using Gnomeshade.WebApi.V1_0.Transactions;

using NodaTime;

namespace Gnomeshade.WebApi.Tests.Integration.V1_0.Transactions;

[TestOf(typeof(TransactionsController))]
public class TransactionsControllerTests
{
	private IGnomeshadeClient _client = null!;

	private Counterparty _counterparty = null!;
	private Counterparty _otherCounterparty = null!;
	private Account _account1 = null!;
	private Account _account2 = null!;
	private Guid _productId;

	[SetUp]
	public async Task SetUpAsync()
	{
		_client = await WebserverSetup.CreateAuthorizedClientAsync();

		_counterparty = await _client.GetMyCounterpartyAsync();
		var other = (await _client.GetCounterpartiesAsync())
			.FirstOrDefault(counterparty => counterparty.Id != _counterparty.Id);

		if (other is null)
		{
			var counterpartyCreation = new CounterpartyCreation { Name = $"{Guid.NewGuid():N}" };
			await _client.PutCounterpartyAsync(Guid.NewGuid(), counterpartyCreation);
			other = (await _client.GetCounterpartiesAsync())
				.First(counterparty => counterparty.Id != _counterparty.Id);
		}

		_otherCounterparty = other;
		var currencies = await _client.GetCurrenciesAsync();
		var currency1 = currencies.First();
		var currency2 = currencies.Skip(1).First();

		_account1 = await CreateAccountAsync(currency1, _counterparty);
		_account2 = await CreateAccountAsync(currency2, _counterparty);

		var productCreation = new ProductCreation { Name = Guid.NewGuid().ToString("N") };
		_productId = Guid.NewGuid();
		await _client.PutProductAsync(_productId, productCreation);
	}

	[Test]
	public async Task PutTransaction()
	{
		var transactionCreationModel = new TransactionCreation
		{
			ValuedAt = SystemClock.Instance.GetCurrentInstant(),
		};

		var transactionId = Guid.NewGuid();
		await _client.PutTransactionAsync(transactionId, transactionCreationModel);
		var transaction = await _client.GetTransactionAsync(transactionId);
		var transactions = await _client.GetTransactionsAsync(new(null, null));
		var detailedTransactions = await _client.GetDetailedTransactionsAsync(new(null, null));

		transactions.Should().ContainSingle(t => t.Id == transactionId).Which.Should().BeEquivalentTo(transaction);
		detailedTransactions.Should().ContainSingle(t => t.Id == transactionId).Which.Should().BeEquivalentTo(transaction);

		transactionCreationModel = transactionCreationModel with
		{
		};

		await _client.PutTransactionAsync(transactionId, transactionCreationModel);
		var identicalTransaction = await _client.GetTransactionAsync(transactionId);

		transaction.Should().BeEquivalentTo(identicalTransaction, WithoutModifiedAt);
	}

	[Test]
	public async Task Transfers()
	{
		var transactionCreationModel = new TransactionCreation
		{
			ValuedAt = SystemClock.Instance.GetCurrentInstant(),
		};

		var transactionId = await _client.CreateTransactionAsync(transactionCreationModel);

		var transferId = Guid.NewGuid();
		var transferCreation = new TransferCreation
		{
			TransactionId = transactionId,
			SourceAmount = 1,
			SourceAccountId = _account1.Currencies.First().Id,
			TargetAmount = 1,
			TargetAccountId = _account2.Currencies.First().Id,
		};

		await _client.PutTransferAsync(transferId, transferCreation);
		var transfer = await _client.GetTransferAsync(transferId);
		var transfers = await _client.GetTransfersAsync(transactionId);
		var detailedTransaction = await _client.GetDetailedTransactionAsync(transactionId);

		transfers.Should().ContainSingle().Which.Should().BeEquivalentTo(transfer);
		detailedTransaction.Transfers.Should().ContainSingle().Which.Should().BeEquivalentTo(transfer);
		transfer.Should().BeEquivalentTo(transferCreation, options => options.Excluding(creation => creation.OwnerId));

		var bankReference = $"{Guid.NewGuid():N}";
		transferCreation = transferCreation with { BankReference = bankReference };
		await _client.PutTransferAsync(transferId, transferCreation);
		(await _client.GetTransferAsync(transferId)).BankReference.Should().Be(bankReference);

		await _client.DeleteTransferAsync(transferId);
		(await _client.GetTransfersAsync(transactionId)).Should().BeEmpty();
		(await FluentActions
				.Awaiting(() => _client.GetTransferAsync(transferId))
				.Should()
				.ThrowExactlyAsync<HttpRequestException>())
			.Which.StatusCode.Should()
			.Be(HttpStatusCode.NotFound);
	}

	[Test]
	public async Task PutTransfer_NonExistentTransaction()
	{
		var transferId = Guid.NewGuid();
		var transferCreation = new TransferCreation
		{
			TransactionId = Guid.NewGuid(),
			SourceAmount = 1,
			SourceAccountId = _account1.Currencies.First().Id,
			TargetAmount = 1,
			TargetAccountId = _account2.Currencies.First().Id,
		};

		(await FluentActions
				.Awaiting(() => _client.PutTransferAsync(transferId, transferCreation))
				.Should()
				.ThrowExactlyAsync<HttpRequestException>())
			.Which.StatusCode.Should()
			.Be(HttpStatusCode.NotFound);
	}

	[Test]
	public async Task Purchases()
	{
		var transactionCreationModel = new TransactionCreation
		{
			ValuedAt = SystemClock.Instance.GetCurrentInstant(),
		};

		var transactionId = await _client.CreateTransactionAsync(transactionCreationModel);

		var purchaseId = Guid.NewGuid();
		var purchaseCreation = new PurchaseCreation
		{
			Price = 2.53m,
			CurrencyId = _account1.Currencies.First().Currency.Id,
			ProductId = _productId,
			Amount = 1,
		};

		await _client.PutPurchaseAsync(transactionId, purchaseId, purchaseCreation);
		var purchase = await _client.GetPurchaseAsync(transactionId, purchaseId);
		var purchases = await _client.GetPurchasesAsync(transactionId);
		var productPurchases = await _client.GetProductPurchasesAsync(_productId);
		var detailedTransaction = await _client.GetDetailedTransactionAsync(transactionId);

		purchases.Should().ContainSingle().Which.Should().BeEquivalentTo(purchase);
		productPurchases.Should().ContainSingle().Which.Should().BeEquivalentTo(purchase);
		detailedTransaction.Purchases.Should().ContainSingle().Which.Should().BeEquivalentTo(purchase);
		purchase.Should().BeEquivalentTo(purchaseCreation, options => options.Excluding(creation => creation.OwnerId));

		var deliveryDate = SystemClock.Instance.GetCurrentInstant();
		purchaseCreation = purchaseCreation with { DeliveryDate = deliveryDate };
		await _client.PutPurchaseAsync(transactionId, purchaseId, purchaseCreation);
		var updatedPurchase = await _client.GetPurchaseAsync(transactionId, purchaseId);
		updatedPurchase.DeliveryDate!.Value.Should().BeInRange(
			deliveryDate - Duration.FromNanoseconds(1000),
			deliveryDate + Duration.FromNanoseconds(1000));

		await _client.DeletePurchaseAsync(transactionId, purchaseId);
		(await _client.GetPurchasesAsync(transactionId)).Should().BeEmpty();
		(await FluentActions
				.Awaiting(() => _client.GetPurchaseAsync(transactionId, purchaseId))
				.Should()
				.ThrowExactlyAsync<HttpRequestException>())
			.Which.StatusCode.Should()
			.Be(HttpStatusCode.NotFound);
	}

	[Test]
	public async Task PutPurchase_NonExistentTransaction()
	{
		var transactionId = Guid.NewGuid();
		var purchaseId = Guid.NewGuid();
		var purchaseCreation = new PurchaseCreation
		{
			Price = 2.53m,
			CurrencyId = _account1.Currencies.First().Currency.Id,
			ProductId = _productId,
			Amount = 1,
		};

		(await FluentActions
				.Awaiting(() => _client.PutPurchaseAsync(transactionId, purchaseId, purchaseCreation))
				.Should()
				.ThrowExactlyAsync<HttpRequestException>())
			.Which.StatusCode.Should()
			.Be(HttpStatusCode.NotFound);
	}

	[Test]
	public async Task Links()
	{
		var transactionCreation = new TransactionCreation { ValuedAt = SystemClock.Instance.GetCurrentInstant() };
		var transactionId = await _client.CreateTransactionAsync(transactionCreation);

		var uri = $"https://localhost/documents/{Guid.NewGuid()}";
		var linkCreation = new LinkCreation { Uri = new(uri) };
		var linkId = Guid.NewGuid();
		await _client.PutLinkAsync(linkId, linkCreation);

		(await _client.GetTransactionLinksAsync(transactionId)).Should().BeEmpty();

		await _client.AddLinkToTransactionAsync(transactionId, linkId);
		var link = (await _client.GetTransactionLinksAsync(transactionId))
			.Should()
			.ContainSingle().Subject;
		var detailedTransaction = await _client.GetDetailedTransactionAsync(transactionId);

		link.Id.Should().Be(linkId);
		link.Uri.Should().Be(uri);
		detailedTransaction.Links.Should().ContainSingle().Which.Should().BeEquivalentTo(link);

		await _client.RemoveLinkFromTransactionAsync(transactionId, linkId);
		(await _client.GetTransactionLinksAsync(transactionId)).Should().BeEmpty();
	}

	[Test]
	public async Task Loans()
	{
		var transactionCreationModel = new TransactionCreation
		{
			ValuedAt = SystemClock.Instance.GetCurrentInstant(),
		};

		var transactionId = await _client.CreateTransactionAsync(transactionCreationModel);
		var receiver = _counterparty;
		var issuer = _otherCounterparty;

		var loanId = Guid.NewGuid();
		var loanCreation = new LoanCreation
		{
			IssuingCounterpartyId = issuer.Id,
			ReceivingCounterpartyId = receiver.Id,
			CurrencyId = _account1.Currencies.First().Currency.Id,
			Amount = 1,
		};

		await _client.PutLoanAsync(transactionId, loanId, loanCreation);
		var loan = await _client.GetLoanAsync(transactionId, loanId);
		var loans = await _client.GetLoansAsync(transactionId);
		var receiverLoans = await _client.GetCounterpartyLoansAsync(receiver.Id);
		var issuerLoans = await _client.GetCounterpartyLoansAsync(issuer.Id);
		var detailedTransaction = await _client.GetDetailedTransactionAsync(transactionId);

		loans.Should().ContainSingle().Which.Should().BeEquivalentTo(loan);
		receiverLoans.Should().ContainSingle().Which.Should().BeEquivalentTo(loan);
		issuerLoans.Should().ContainSingle().Which.Should().BeEquivalentTo(loan);
		detailedTransaction.Loans.Should().ContainSingle().Which.Should().BeEquivalentTo(loan);
		loan.Should().BeEquivalentTo(loanCreation, options => options.Excluding(creation => creation.OwnerId));

		loanCreation = loanCreation with { Amount = 2 };
		await _client.PutLoanAsync(transactionId, loanId, loanCreation);
		var updatedLoan = await _client.GetLoanAsync(transactionId, loanId);
		updatedLoan.Amount.Should().Be(2);

		await _client.DeleteLoanAsync(transactionId, loanId);
		(await _client.GetLoansAsync(transactionId)).Should().BeEmpty();
		(await FluentActions
				.Awaiting(() => _client.GetLoanAsync(transactionId, loanId))
				.Should()
				.ThrowExactlyAsync<HttpRequestException>())
			.Which.StatusCode.Should()
			.Be(HttpStatusCode.NotFound);
	}

	[Test]
	public async Task PutLoan_NonExistentTransaction()
	{
		var transactionId = Guid.NewGuid();
		var counterparties = await _client.GetCounterpartiesAsync();
		var receiver = await _client.GetMyCounterpartyAsync();
		var issuer = counterparties.First(counterparty => counterparty.Id != receiver.Id);

		var loanId = Guid.NewGuid();
		var loanCreation = new LoanCreation
		{
			IssuingCounterpartyId = issuer.Id,
			ReceivingCounterpartyId = receiver.Id,
			CurrencyId = _account1.Currencies.First().Currency.Id,
			Amount = 1,
		};

		(await FluentActions
				.Awaiting(() => _client.PutLoanAsync(transactionId, loanId, loanCreation))
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
			.Excluding(info => info.Name == nameof(IModifiableEntity.ModifiedAt));
	}

	private async Task<Account> CreateAccountAsync(Currency currency, Counterparty counterparty)
	{
		var creationModel = new AccountCreation
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
