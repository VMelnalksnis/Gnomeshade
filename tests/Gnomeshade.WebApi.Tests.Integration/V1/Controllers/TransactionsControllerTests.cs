// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Gnomeshade.TestingHelpers.Models;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models;
using Gnomeshade.WebApi.Models.Accounts;
using Gnomeshade.WebApi.Models.Products;
using Gnomeshade.WebApi.Models.Transactions;
using Gnomeshade.WebApi.Tests.Integration.Fixtures;
using Gnomeshade.WebApi.V1.Controllers;

using NodaTime;

namespace Gnomeshade.WebApi.Tests.Integration.V1.Controllers;

[TestOf(typeof(TransactionsController))]
public sealed class TransactionsControllerTests : WebserverTests
{
	private IGnomeshadeClient _client = null!;

	private Counterparty _counterparty = null!;
	private Counterparty _otherCounterparty = null!;
	private Account _account1 = null!;
	private Account _account2 = null!;
	private Guid _productId;

	public TransactionsControllerTests(WebserverFixture fixture)
		: base(fixture)
	{
	}

	[SetUp]
	public async Task SetUpAsync()
	{
		_client = await Fixture.CreateAuthorizedClientAsync();

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
		var transactionCreationModel = new TransactionCreation();

		var transactionId = Guid.NewGuid();
		await _client.PutTransactionAsync(transactionId, transactionCreationModel);

		var detailedTransactions = await _client.GetDetailedTransactionsAsync(new(null, null));
		detailedTransactions.Should().NotContain(detailed => detailed.Id == transactionId);

		var transferCreation = new TransferCreation
		{
			TransactionId = transactionId,
			SourceAmount = 1,
			SourceAccountId = _account1.Currencies.First().Id,
			TargetAmount = 1,
			TargetAccountId = _account2.Currencies.First().Id,
			Order = 2,
			ValuedAt = SystemClock.Instance.GetCurrentInstant(),
		};

		await _client.CreateTransferAsync(transactionId, _account1.Id, _account2.Id);
		var expectedDate = SystemClock.Instance.GetCurrentInstant() + Duration.FromDays(1);
		await _client.PutTransferAsync(Guid.NewGuid(), transferCreation with { ValuedAt = expectedDate, Order = 3 });

		var transaction = await _client.GetTransactionAsync(transactionId);
		var transactions = await _client.GetTransactionsAsync();
		detailedTransactions = await _client.GetDetailedTransactionsAsync(new(Instant.MinValue, Instant.MaxValue));

		transactions.Should().ContainSingle(t => t.Id == transactionId).Which.Should().BeEquivalentTo(transaction);
		var detailed = detailedTransactions.Should().ContainSingle(t => t.Id == transactionId).Subject;
		using (new AssertionScope())
		{
			detailed.Transfers.Should().HaveCount(2);
			detailed.Should().BeEquivalentTo(transaction);
			detailed.ValuedAt?.InUtc().Date.Should().Be(expectedDate.InUtc().Date);
		}

		transactionCreationModel = transactionCreationModel with
		{
		};

		await _client.PutTransactionAsync(transactionId, transactionCreationModel);
		var identicalTransaction = await _client.GetTransactionAsync(transactionId);

		transaction.Should().BeEquivalentTo(identicalTransaction, options => options.WithoutModifiedAt());
	}

	[Test]
	public async Task Transfers()
	{
		var transactionId = await _client.CreateTransactionAsync(new());

		var transferId = Guid.NewGuid();
		var transferCreation = new TransferCreation
		{
			TransactionId = transactionId,
			SourceAmount = 1,
			SourceAccountId = _account1.Currencies.First().Id,
			TargetAmount = 1,
			TargetAccountId = _account2.Currencies.First().Id,
			Order = 2,
			ValuedAt = SystemClock.Instance.GetCurrentInstant(),
		};

		await _client.PutTransferAsync(transferId, transferCreation);
		var transfer = await _client.GetTransferAsync(transferId);
		var transfers = await _client.GetTransfersAsync(transactionId);
		var detailedTransaction = await _client.GetDetailedTransactionAsync(transactionId);

		transfers.Should().ContainSingle().Which.Should().BeEquivalentTo(transfer);
		detailedTransaction.Transfers.Should().ContainSingle().Which.Should().BeEquivalentTo(transfer);

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
			ValuedAt = SystemClock.Instance.GetCurrentInstant(),
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
		var transactionId = await _client.CreateTransactionAsync(new());

		var purchaseId = Guid.NewGuid();
		var purchaseCreation = new PurchaseCreation
		{
			TransactionId = transactionId,
			Price = 2.53m,
			CurrencyId = _account1.Currencies.First().Currency.Id,
			ProductId = _productId,
			Amount = 1,
			Order = 2,
		};

		await _client.PutPurchaseAsync(purchaseId, purchaseCreation);
		var purchase = await _client.GetPurchaseAsync(purchaseId);
		var purchases = await _client.GetPurchasesAsync(transactionId);
		var productPurchases = await _client.GetProductPurchasesAsync(_productId);
		var detailedTransaction = await _client.GetDetailedTransactionAsync(transactionId);

		purchases.Should().ContainSingle().Which.Should().BeEquivalentTo(purchase);
		productPurchases.Should().ContainSingle().Which.Should().BeEquivalentTo(purchase);
		detailedTransaction.Purchases.Should().ContainSingle().Which.Should().BeEquivalentTo(purchase);
		purchase.Should().BeEquivalentTo(purchaseCreation, options => options.Excluding(creation => creation.OwnerId));

		var deliveryDate = SystemClock.Instance.GetCurrentInstant();
		purchaseCreation = purchaseCreation with { DeliveryDate = deliveryDate };
		await _client.PutPurchaseAsync(purchaseId, purchaseCreation);
		var updatedPurchase = await _client.GetPurchaseAsync(purchaseId);
		updatedPurchase.DeliveryDate!.Value.Should().BeInRange(
			deliveryDate - Duration.FromNanoseconds(1000),
			deliveryDate + Duration.FromNanoseconds(1000));

		await _client.DeletePurchaseAsync(purchaseId);
		(await _client.GetPurchasesAsync(transactionId)).Should().BeEmpty();
		(await FluentActions
				.Awaiting(() => _client.GetPurchaseAsync(purchaseId))
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
			TransactionId = transactionId,
			Price = 2.53m,
			CurrencyId = _account1.Currencies.First().Currency.Id,
			ProductId = _productId,
			Amount = 1,
		};

		(await FluentActions
				.Awaiting(() => _client.PutPurchaseAsync(purchaseId, purchaseCreation))
				.Should()
				.ThrowExactlyAsync<HttpRequestException>())
			.Which.StatusCode.Should()
			.Be(HttpStatusCode.NotFound);
	}

	[Test]
	public async Task Links()
	{
		var transactionId = await _client.CreateTransactionAsync(new());

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
		var transactionId = await _client.CreateTransactionAsync(new());
		var receiver = _counterparty;
		var issuer = _otherCounterparty;

		var loanId = Guid.NewGuid();
		var loanCreation = new LoanCreation
		{
			TransactionId = transactionId,
			IssuingCounterpartyId = issuer.Id,
			ReceivingCounterpartyId = receiver.Id,
			CurrencyId = _account1.Currencies.First().Currency.Id,
			Amount = 1,
		};

		await _client.PutLoanAsync(loanId, loanCreation);
		var loan = await _client.GetLoanAsync(loanId);
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
		await _client.PutLoanAsync(loanId, loanCreation);
		var updatedLoan = await _client.GetLoanAsync(loanId);
		updatedLoan.Amount.Should().Be(2);

		await _client.DeleteLoanAsync(loanId);
		(await _client.GetLoansAsync(transactionId)).Should().BeEmpty();
		(await FluentActions
				.Awaiting(() => _client.GetLoanAsync(loanId))
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
			TransactionId = transactionId,
			IssuingCounterpartyId = issuer.Id,
			ReceivingCounterpartyId = receiver.Id,
			CurrencyId = _account1.Currencies.First().Currency.Id,
			Amount = 1,
		};

		(await FluentActions
				.Awaiting(() => _client.PutLoanAsync(loanId, loanCreation))
				.Should()
				.ThrowExactlyAsync<HttpRequestException>())
			.Which.StatusCode.Should()
			.Be(HttpStatusCode.NotFound);
	}

	[Test]
	public async Task Merge()
	{
		var counterparties = await _client.GetCounterpartiesAsync();
		var receiver = await _client.GetMyCounterpartyAsync();
		var issuer = counterparties.First(counterparty => counterparty.Id != receiver.Id);

		var sourceTransaction = await _client.CreateDetailedTransactionAsync(
			_account1.Id,
			_account2.Id,
			_productId,
			issuer.Id,
			receiver.Id);

		var targetTransaction = await _client.CreateDetailedTransactionAsync(
			_account2.Id,
			_account1.Id,
			_productId,
			receiver.Id,
			issuer.Id);

		await _client.MergeTransactionsAsync(targetTransaction.Id, sourceTransaction.Id);

		using (new AssertionScope())
		{
			(await FluentActions
					.Awaiting(() => _client.GetTransactionAsync(sourceTransaction.Id))
					.Should()
					.ThrowExactlyAsync<HttpRequestException>())
				.Which.StatusCode.Should()
				.Be(HttpStatusCode.NotFound);

			var mergedTransaction = await _client.GetDetailedTransactionAsync(targetTransaction.Id);

			mergedTransaction.Transfers.Should()
				.ContainEquivalentOf(sourceTransaction.Transfers.Single(), options => options.WithoutTransaction())
				.And.ContainEquivalentOf(targetTransaction.Transfers.Single(), options => options.WithoutTransaction());

			mergedTransaction.Purchases.Should()
				.ContainEquivalentOf(sourceTransaction.Purchases.Single(), options => options.WithoutTransaction())
				.And.ContainEquivalentOf(targetTransaction.Purchases.Single(), options => options.WithoutTransaction());

			mergedTransaction.Loans.Should()
				.ContainEquivalentOf(sourceTransaction.Loans.Single(), options => options.WithoutTransaction())
				.And.ContainEquivalentOf(targetTransaction.Loans.Single(), options => options.WithoutTransaction());

			mergedTransaction.Links.Should()
				.ContainEquivalentOf(sourceTransaction.Links.Single(), options => options.WithoutModifiedAt())
				.And.ContainEquivalentOf(targetTransaction.Links.Single(), options => options.WithoutModifiedAt());
		}
	}

	[Test]
	public async Task Related()
	{
		var transaction = await _client.CreateTransactionAsync();
		var relatedTransaction = await _client.CreateTransactionAsync();

		(await _client.GetRelatedTransactionAsync(transaction.Id)).Should().BeEmpty();

		await _client.AddRelatedTransactionAsync(transaction.Id, relatedTransaction.Id);

		(await _client.GetRelatedTransactionAsync(transaction.Id))
			.Should()
			.ContainSingle()
			.Which.Should()
			.BeEquivalentTo(relatedTransaction, options => options.WithoutModifiedAt());

		await _client.RemoveRelatedTransactionAsync(transaction.Id, relatedTransaction.Id);

		(await _client.GetRelatedTransactionAsync(transaction.Id)).Should().BeEmpty();
	}

	[Test]
	public async Task Detailed()
	{
		var transaction = await _client.CreateTransactionAsync();
		var transfer = await _client.CreateTransferAsync(transaction.Id, _account1.Id, _account2.Id);
		var purchase = await _client.CreatePurchaseAsync(transaction.Id, _productId);
		await _client.CreatePurchaseAsync(transaction.Id, _productId);
		var loan = await _client.CreateLoanAsync(transaction.Id, _counterparty.Id, _otherCounterparty.Id);
		var linkCreation = new LinkCreation { Uri = new($"https://localhost/documents/{Guid.NewGuid()}") };
		var linkId = Guid.NewGuid();
		await _client.PutLinkAsync(linkId, linkCreation);
		await _client.AddLinkToTransactionAsync(transaction.Id, linkId);

		var detailed = await _client.GetDetailedTransactionAsync(transaction.Id);
		using (new AssertionScope())
		{
			detailed.Should().BeEquivalentTo(transaction);
			detailed.Transfers.Should().ContainSingle().Which.Should().BeEquivalentTo(transfer);
			detailed.Purchases.Should().HaveCount(2).And.ContainEquivalentOf(purchase);
			detailed.Loans.Should().ContainSingle().Which.Should().BeEquivalentTo(loan);
			detailed.Links.Should().ContainSingle().Which.Id.Should().Be(linkId);
		}

		await _client.DeleteTransferAsync(transfer.Id);
		await _client.DeletePurchaseAsync(purchase.Id);
		await _client.DeleteLoanAsync(loan.Id);
		await _client.DeleteLinkAsync(linkId);

		detailed = await _client.GetDetailedTransactionAsync(transaction.Id);
		using (new AssertionScope())
		{
			detailed.Should().BeEquivalentTo(transaction);
			detailed.Transfers.Should().BeEmpty();
			detailed.Purchases.Should().HaveCount(1);
			detailed.Loans.Should().BeEmpty();
			detailed.Links.Should().BeEmpty();
		}
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
