// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models;
using Gnomeshade.Interfaces.WebApi.Models.Accounts;
using Gnomeshade.Interfaces.WebApi.Models.Products;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;
using Gnomeshade.TestingHelpers.Models;

using NodaTime;

namespace Gnomeshade.Interfaces.WebApi.Tests.Integration.AccessControl;

public sealed class PrivateByDefaultTests
{
	private IGnomeshadeClient _client = null!;
	private IGnomeshadeClient _otherClient = null!;

	[OneTimeSetUp]
	public async Task OneTimeSetUp()
	{
		_client = await WebserverSetup.CreateAuthorizedClientAsync();
		_otherClient = await WebserverSetup.CreateAuthorizedSecondClientAsync();
	}

	[Test]
	public async Task Counterparties()
	{
		var counterparty = await _client.CreateCounterpartyAsync();

		await ShouldBeNotFoundForOthers(client => client.GetCounterpartyAsync(counterparty.Id));

		var counterpartyCreation = new CounterpartyCreation { Name = $"{counterparty.Name}1" };

		await ShouldBeForbiddenForOthers(client => client.PutCounterpartyAsync(counterparty.Id, counterpartyCreation));
		await ShouldBeNotFoundForOthers(client => client.GetCounterpartyAsync(counterparty.Id));
	}

	[Test]
	public async Task Accounts()
	{
		var counterpartyId = await _client.CreateCounterpartyAsync();
		var account = await CreateAccountAsync(_client, counterpartyId.Id);

		await ShouldBeNotFoundForOthers(client => client.GetAccountAsync(account.Id));

		var accountCreation = account.ToCreation() with { Name = $"{account.Name}1" };

		await ShouldBeForbiddenForOthers(client => client.PutAccountAsync(account.Id, accountCreation));
		await ShouldBeNotFoundForOthers(client => client.GetAccountAsync(account.Id));
	}

	[Test]
	public async Task Categories()
	{
		var category = await _client.CreateCategoryAsync();

		await ShouldBeNotFoundForOthers(client => client.GetCategoryAsync(category.Id));

		var updatedCategory = category.ToCreation() with { Name = $"{category.Name}1" };

		await ShouldBeForbiddenForOthers(client => client.PutCategoryAsync(category.Id, updatedCategory));
		await ShouldBeNotFoundForOthers(client => client.GetCategoryAsync(category.Id));
		await ShouldBeNotFoundForOthers(client => client.DeleteCategoryAsync(category.Id));
	}

	[Test]
	public async Task Units()
	{
		var unit = await CreateUnitAsync(_client);

		await ShouldBeNotFoundForOthers(client => client.GetUnitAsync(unit.Id));

		var updatedUnit = unit.ToCreation() with { Name = $"{unit.Name}1" };

		await ShouldBeForbiddenForOthers(client => client.PutUnitAsync(unit.Id, updatedUnit));
		await ShouldBeNotFoundForOthers(client => client.GetUnitAsync(unit.Id));
	}

	[Test]
	public async Task Products()
	{
		var unit = await CreateUnitAsync(_client);
		var category = await _client.CreateCategoryAsync();
		var product = await CreateProductAsync(_client, unit.Id, category.Id);

		await ShouldBeNotFoundForOthers(client => client.GetProductAsync(product.Id));

		var updatedProduct = product.ToCreation() with { Name = $"{category.Name}1" };

		await ShouldBeForbiddenForOthers(client => client.PutProductAsync(category.Id, updatedProduct));
		await ShouldBeNotFoundForOthers(client => client.GetProductAsync(product.Id));
	}

	[Test]
	public async Task Transactions()
	{
		var transaction = await _client.CreateTransactionAsync();

		await ShouldBeNotFoundForOthers(client => client.GetTransactionAsync(transaction.Id));

		var updatedTransaction = transaction.ToCreation() with { ValuedAt = SystemClock.Instance.GetCurrentInstant() };

		await ShouldBeForbiddenForOthers(client => client.PutTransactionAsync(transaction.Id, updatedTransaction));
		await ShouldBeNotFoundForOthers(client => client.GetTransactionAsync(transaction.Id));
		await ShouldBeNotFoundForOthers(client => client.DeleteTransactionAsync(transaction.Id));
	}

	[Test]
	public async Task Transfers()
	{
		var transaction = await _client.CreateTransactionAsync();
		var counterpartyId = await _client.CreateCounterpartyAsync();
		var account1 = await CreateAccountAsync(_client, counterpartyId.Id);
		var account2 = await CreateAccountAsync(_client, counterpartyId.Id);

		var transfer = await CreateTransferAsync(_client, transaction.Id, account1.Id, account2.Id);

		await ShouldBeNotFoundForOthers(client => client.GetTransferAsync(transaction.Id, transfer.Id));

		var updatedTransfer = transfer.ToCreation() with { BankReference = $"{transfer.BankReference}1" };

		await ShouldBeForbiddenForOthers(client => client.PutTransferAsync(transaction.Id, transfer.Id, updatedTransfer));
		await ShouldBeNotFoundForOthers(client => client.GetTransferAsync(transaction.Id, transfer.Id));
		await ShouldBeNotFoundForOthers(client => client.DeleteTransferAsync(transaction.Id, transfer.Id), true);
	}

	[Test]
	public async Task Purchases()
	{
		var transaction = await _client.CreateTransactionAsync();
		var unit = await CreateUnitAsync(_client);
		var category = await _client.CreateCategoryAsync();
		var product = await CreateProductAsync(_client, unit.Id, category.Id);

		var purchase = await CreatePurchaseAsync(_client, transaction.Id, product.Id);

		await ShouldBeNotFoundForOthers(client => client.GetPurchaseAsync(transaction.Id, purchase.Id));

		var updatedTransfer = purchase.ToCreation() with { Amount = purchase.Amount + 1 };

		await ShouldBeForbiddenForOthers(client => client.PutPurchaseAsync(transaction.Id, purchase.Id, updatedTransfer));
		await ShouldBeNotFoundForOthers(client => client.GetPurchaseAsync(transaction.Id, purchase.Id));
		await ShouldBeNotFoundForOthers(client => client.DeletePurchaseAsync(transaction.Id, purchase.Id), true);
	}

	[Test]
	public async Task Loans()
	{
		var transaction = await _client.CreateTransactionAsync();
		var counterparty1 = await _client.GetMyCounterpartyAsync();
		var counterparty2 = await _otherClient.GetMyCounterpartyAsync();

		var loan = await CreateLoanAsync(_client, transaction.Id, counterparty1.Id, counterparty2.Id);

		await ShouldBeNotFoundForOthers(client => client.GetLoanAsync(transaction.Id, loan.Id));

		var updatedLoan = loan.ToCreation() with { Amount = loan.Amount + 1 };

		await ShouldBeForbiddenForOthers(client => client.PutLoanAsync(transaction.Id, loan.Id, updatedLoan));
		await ShouldBeNotFoundForOthers(client => client.GetLoanAsync(transaction.Id, loan.Id));
		await ShouldBeNotFoundForOthers(client => client.DeleteLoanAsync(transaction.Id, loan.Id), true);
	}

	[Test]
	public async Task Links()
	{
		var linkId = Guid.NewGuid();
		await _client.PutLinkAsync(linkId, new() { Uri = new("https://localhost/") });
		var link = await _client.GetLinkAsync(linkId);

		await ShouldBeNotFoundForOthers(client => client.GetLinkAsync(link.Id));

		var updatedLink = new LinkCreation { Uri = new("https://localhost/test") };

		await ShouldBeForbiddenForOthers(client => client.PutLinkAsync(link.Id, updatedLink));
		await ShouldBeNotFoundForOthers(client => client.GetLinkAsync(link.Id));
		await ShouldBeNotFoundForOthers(client => client.DeleteLinkAsync(link.Id), true);
	}

	private static async Task<Account> CreateAccountAsync(IAccountClient accountClient, Guid counterpartyId)
	{
		var currency = (await accountClient.GetCurrenciesAsync()).First();
		var accountId = Guid.NewGuid();
		var account = new AccountCreation
		{
			Name = $"{accountId:N}",
			CounterpartyId = counterpartyId,
			PreferredCurrencyId = currency.Id,
			Currencies = new() { new() { CurrencyId = currency.Id } },
		};

		await accountClient.PutAccountAsync(accountId, account);
		return await accountClient.GetAccountAsync(accountId);
	}

	private static async Task<Unit> CreateUnitAsync(IProductClient productClient)
	{
		var unitId = Guid.NewGuid();
		var unit = new UnitCreation { Name = $"{unitId:N}" };
		await productClient.PutUnitAsync(unitId, unit);
		return await productClient.GetUnitAsync(unitId);
	}

	private static async Task<Product> CreateProductAsync(IProductClient productClient, Guid unitId, Guid categoryId)
	{
		var productId = Guid.NewGuid();
		var product = new ProductCreation { Name = $"{productId:N}", UnitId = unitId, CategoryId = categoryId };
		await productClient.PutProductAsync(productId, product);
		return await productClient.GetProductAsync(productId);
	}

	private static async Task<Transfer> CreateTransferAsync(
		IGnomeshadeClient gnomeshadeClient,
		Guid transactionId,
		Guid account1Id,
		Guid account2Id)
	{
		var account1 = await gnomeshadeClient.GetAccountAsync(account1Id);
		var account2 = await gnomeshadeClient.GetAccountAsync(account2Id);

		var transferId = Guid.NewGuid();
		var transfer = new TransferCreation
		{
			SourceAccountId = account1.Currencies.First().Id,
			TargetAccountId = account2.Currencies.First().Id,
			SourceAmount = 10.5m,
			TargetAmount = 10.5m,
		};

		await gnomeshadeClient.PutTransferAsync(transactionId, transferId, transfer);
		return await gnomeshadeClient.GetTransferAsync(transactionId, transferId);
	}

	private static async Task<Purchase> CreatePurchaseAsync(
		IGnomeshadeClient gnomeshadeClient,
		Guid transactionId,
		Guid productId)
	{
		var currency = (await gnomeshadeClient.GetCurrenciesAsync()).First();
		var purchaseId = Guid.NewGuid();
		var purchase = new PurchaseCreation
		{
			CurrencyId = currency.Id,
			ProductId = productId,
			Price = 10.5m,
			Amount = 1,
		};

		await gnomeshadeClient.PutPurchaseAsync(transactionId, purchaseId, purchase);
		return await gnomeshadeClient.GetPurchaseAsync(transactionId, purchaseId);
	}

	private static async Task<Loan> CreateLoanAsync(
		IGnomeshadeClient gnomeshadeClient,
		Guid transactionId,
		Guid issuingId,
		Guid receivingId)
	{
		var currency = (await gnomeshadeClient.GetCurrenciesAsync()).First();
		var loanId = Guid.NewGuid();
		var purchase = new LoanCreation
		{
			CurrencyId = currency.Id,
			Amount = 1,
			IssuingCounterpartyId = issuingId,
			ReceivingCounterpartyId = receivingId,
		};

		await gnomeshadeClient.PutLoanAsync(transactionId, loanId, purchase);
		return await gnomeshadeClient.GetLoanAsync(transactionId, loanId);
	}

	private Task ShouldBeNotFoundForOthers(Func<IGnomeshadeClient, Task> func, bool inverted = false)
	{
		return ShouldReturnStatusCode(func, HttpStatusCode.NotFound, inverted);
	}

	private Task ShouldBeForbiddenForOthers(Func<IGnomeshadeClient, Task> func, bool inverted = false)
	{
		return ShouldReturnStatusCode(func, HttpStatusCode.Forbidden, inverted);
	}

	private async Task ShouldReturnStatusCode(
		Func<IGnomeshadeClient, Task> func,
		HttpStatusCode statusCode,
		bool inverted)
	{
		if (inverted)
		{
			(await FluentActions
					.Awaiting(() => func(_otherClient))
					.Should()
					.ThrowAsync<HttpRequestException>())
				.Which.StatusCode.Should()
				.Be(statusCode);

			await FluentActions
				.Awaiting(() => func(_client))
				.Should()
				.NotThrowAsync();
		}
		else
		{
			await FluentActions
				.Awaiting(() => func(_client))
				.Should()
				.NotThrowAsync();

			(await FluentActions
					.Awaiting(() => func(_otherClient))
					.Should()
					.ThrowAsync<HttpRequestException>())
				.Which.StatusCode.Should()
				.Be(statusCode);
		}
	}
}
