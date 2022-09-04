// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Gnomeshade.TestingHelpers.Models;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models;
using Gnomeshade.WebApi.Models.Accounts;
using Gnomeshade.WebApi.Tests.Integration.Fixtures;

using NodaTime;

namespace Gnomeshade.WebApi.Tests.Integration.AccessControl;

public sealed class PrivateByDefaultTests : WebserverTests
{
	private IGnomeshadeClient _client = null!;
	private IGnomeshadeClient _otherClient = null!;

	public PrivateByDefaultTests(WebserverFixture fixture)
		: base(fixture)
	{
	}

	[OneTimeSetUp]
	public async Task OneTimeSetUp()
	{
		_client = await Fixture.CreateAuthorizedClientAsync();
		_otherClient = await Fixture.CreateAuthorizedSecondClientAsync();
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
		var counterparty = await _client.CreateCounterpartyAsync();
		var account = await _client.CreateAccountAsync(counterparty.Id);

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
		var unit = await _client.CreateUnitAsync();

		await ShouldBeNotFoundForOthers(client => client.GetUnitAsync(unit.Id));

		var updatedUnit = unit.ToCreation() with { Name = $"{unit.Name}1" };

		await ShouldBeForbiddenForOthers(client => client.PutUnitAsync(unit.Id, updatedUnit));
		await ShouldBeNotFoundForOthers(client => client.GetUnitAsync(unit.Id));
	}

	[Test]
	public async Task Products()
	{
		var unit = await _client.CreateUnitAsync();
		var category = await _client.CreateCategoryAsync();
		var product = await _client.CreateProductAsync(unit.Id, category.Id);

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
		await ShouldBeNotFoundForOthers(client => client.GetDetailedTransactionAsync(transaction.Id));
		(await _client.GetTransactionsAsync(new(null, null))).Should().ContainSingle(t => t.Id == transaction.Id);
		(await _client.GetDetailedTransactionsAsync(new(null, null))).Should().ContainSingle(t => t.Id == transaction.Id);

		var updatedTransaction = transaction.ToCreation() with { ValuedAt = SystemClock.Instance.GetCurrentInstant() };

		await ShouldBeForbiddenForOthers(client => client.PutTransactionAsync(transaction.Id, updatedTransaction));
		await ShouldBeNotFoundForOthers(client => client.GetTransactionAsync(transaction.Id));
		await ShouldBeNotFoundForOthers(client => client.GetDetailedTransactionAsync(transaction.Id));
		await ShouldBeNotFoundForOthers(client => client.DeleteTransactionAsync(transaction.Id));
	}

	[Test]
	public async Task Transfers()
	{
		var transaction = await _client.CreateTransactionAsync();
		var counterparty = await _client.CreateCounterpartyAsync();
		var account1 = await _client.CreateAccountAsync(counterparty.Id);
		var account2 = await _client.CreateAccountAsync(counterparty.Id);

		var transfer = await _client.CreateTransferAsync(transaction.Id, account1.Id, account2.Id);

		await ShouldBeNotFoundForOthers(client => client.GetTransferAsync(transfer.Id));

		var updatedTransfer = transfer.ToCreation() with { BankReference = $"{transfer.BankReference}1" };

		await ShouldBeForbiddenForOthers(client => client.PutTransferAsync(transfer.Id, updatedTransfer));
		await ShouldBeNotFoundForOthers(client => client.GetTransferAsync(transfer.Id));
		await ShouldBeNotFoundForOthers(client => client.DeleteTransferAsync(transfer.Id), true);
	}

	[Test]
	public async Task Purchases()
	{
		var transaction = await _client.CreateTransactionAsync();
		var unit = await _client.CreateUnitAsync();
		var category = await _client.CreateCategoryAsync();
		var product = await _client.CreateProductAsync(unit.Id, category.Id);

		var purchase = await _client.CreatePurchaseAsync(transaction.Id, product.Id);

		await ShouldBeNotFoundForOthers(client => client.GetPurchaseAsync(purchase.Id));

		var updatedTransfer = purchase.ToCreation() with { Amount = purchase.Amount + 1 };

		await ShouldBeForbiddenForOthers(client => client.PutPurchaseAsync(purchase.Id, updatedTransfer));
		await ShouldBeNotFoundForOthers(client => client.GetPurchaseAsync(purchase.Id));
		await ShouldBeNotFoundForOthers(client => client.DeletePurchaseAsync(purchase.Id), true);
	}

	[Test]
	public async Task Loans()
	{
		var transaction = await _client.CreateTransactionAsync();
		var counterparty1 = await _client.GetMyCounterpartyAsync();
		var counterparty2 = await _otherClient.GetMyCounterpartyAsync();

		var loan = await _client.CreateLoanAsync(transaction.Id, counterparty1.Id, counterparty2.Id);

		await ShouldBeNotFoundForOthers(client => client.GetLoanAsync(loan.Id));

		var updatedLoan = loan.ToCreation() with { Amount = loan.Amount + 1 };

		await ShouldBeForbiddenForOthers(client => client.PutLoanAsync(loan.Id, updatedLoan));
		await ShouldBeNotFoundForOthers(client => client.GetLoanAsync(loan.Id));
		await ShouldBeNotFoundForOthers(client => client.DeleteLoanAsync(loan.Id), true);
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
