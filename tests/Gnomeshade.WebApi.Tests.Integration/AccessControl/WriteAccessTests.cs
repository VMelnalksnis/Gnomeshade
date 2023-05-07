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
using Gnomeshade.WebApi.Models.Owners;
using Gnomeshade.WebApi.Tests.Integration.Fixtures;

namespace Gnomeshade.WebApi.Tests.Integration.AccessControl;

[TestFixtureSource(typeof(OwnerTestFixtureSource))]
public sealed class WriteAccessTests : WebserverTests
{
	private readonly Func<IGnomeshadeClient, Task<Guid>> _ownerIdFunc;
	private readonly Guid _writeOwnershipId = Guid.NewGuid();

	private IGnomeshadeClient _client = null!;
	private IGnomeshadeClient _otherClient = null!;
	private Guid? _ownerId;

	public WriteAccessTests(Func<IGnomeshadeClient, Task<Guid>> ownerIdFunc, WebserverFixture fixture)
		: base(fixture)
	{
		_ownerIdFunc = ownerIdFunc;
	}

	[OneTimeSetUp]
	public async Task OneTimeSetUp()
	{
		_client = await Fixture.CreateAuthorizedClientAsync();
		_otherClient = await Fixture.CreateAuthorizedClientAsync();

		var counterparty = await _client.GetMyCounterpartyAsync();
		var otherCounterparty = await _otherClient.GetMyCounterpartyAsync();
		var accesses = await _client.GetAccessesAsync();
		var writeAccess = accesses.Single(access => access.Name == "Write");
		var ownerId = await _ownerIdFunc(_client);
		var writeOwnership = new OwnershipCreation
		{
			AccessId = writeAccess.Id,
			OwnerId = ownerId,
			UserId = otherCounterparty.OwnerId,
		};

		await _client.PutOwnershipAsync(_writeOwnershipId, writeOwnership);

		_ownerId = ownerId == counterparty.OwnerId ? null : ownerId;
	}

	[OneTimeTearDown]
	public async Task OneTimeTearDown()
	{
		await _client.DeleteOwnershipAsync(_writeOwnershipId);
	}

	[Test]
	public async Task Counterparties()
	{
		var counterparty = await _client.CreateCounterpartyAsync(_ownerId);

		await ShouldBeNotFoundForOthers(client => client.GetCounterpartyAsync(counterparty.Id));

		var counterpartyCreation = new CounterpartyCreation { Name = $"{counterparty.Name}1" };

		await _otherClient.PutCounterpartyAsync(counterparty.Id, counterpartyCreation);
		var updatedCounterparty = await _client.GetCounterpartyAsync(counterparty.Id);
		updatedCounterparty.Name.Should().Be(counterpartyCreation.Name);

		await ShouldBeNotFoundForOthers(client => client.GetCounterpartyAsync(counterparty.Id));
	}

	[Test]
	public async Task Accounts()
	{
		var counterpartyId = await _client.CreateCounterpartyAsync(_ownerId);
		var account = await _client.CreateAccountAsync(counterpartyId.Id, _ownerId);

		await ShouldBeNotFoundForOthers(client => client.GetAccountAsync(account.Id));

		var accountCreation = account.ToCreation() with { Name = $"{account.Name}1" };

		await _otherClient.PutAccountAsync(account.Id, accountCreation);
		var updatedAccount = await _client.GetAccountAsync(account.Id);
		updatedAccount.Name.Should().Be(accountCreation.Name);

		await ShouldBeNotFoundForOthers(client => client.GetAccountAsync(account.Id));
	}

	[Test]
	public async Task Categories()
	{
		var category = await _client.CreateCategoryAsync(_ownerId);

		await ShouldBeNotFoundForOthers(client => client.GetCategoryAsync(category.Id));

		var categoryCreation = category.ToCreation() with { Name = $"{category.Name}1" };

		await _otherClient.PutCategoryAsync(category.Id, categoryCreation);
		var updatedAccount = await _client.GetCategoryAsync(category.Id);
		updatedAccount.Name.Should().Be(categoryCreation.Name);

		await ShouldBeNotFoundForOthers(client => client.GetCategoryAsync(category.Id));
	}

	[Test]
	public async Task Units()
	{
		var unit = await _client.CreateUnitAsync(_ownerId);

		await ShouldBeNotFoundForOthers(client => client.GetUnitAsync(unit.Id));

		var unitCreation = unit.ToCreation() with { Name = $"{unit.Name}1" };

		await _otherClient.PutUnitAsync(unit.Id, unitCreation);
		var updatedUnit = await _client.GetUnitAsync(unit.Id);
		updatedUnit.Name.Should().Be(unitCreation.Name);

		await ShouldBeNotFoundForOthers(client => client.GetUnitAsync(unit.Id));
	}

	[Test]
	public async Task Products()
	{
		var unit = await _client.CreateUnitAsync(_ownerId);
		var category = await _client.CreateCategoryAsync(_ownerId);
		var product = await _client.CreateProductAsync(unit.Id, category.Id, _ownerId);

		await ShouldBeNotFoundForOthers(client => client.GetProductAsync(product.Id));

		var productCreation = product.ToCreation() with { Name = $"{product.Name}1" };

		await _otherClient.PutProductAsync(product.Id, productCreation);
		var updatedProduct = await _client.GetProductAsync(product.Id);
		updatedProduct.Name.Should().Be(productCreation.Name);

		await ShouldBeNotFoundForOthers(client => client.GetProductAsync(product.Id));
	}

	[Test]
	public async Task Transactions()
	{
		var transaction = await _client.CreateTransactionAsync(_ownerId);

		await ShouldBeNotFoundForOthers(client => client.GetTransactionAsync(transaction.Id));

		var transactionCreation = transaction.ToCreation() with { Description = Guid.NewGuid().ToString() };

		await _otherClient.PutTransactionAsync(transaction.Id, transactionCreation);
		var updatedTransaction = await _client.GetTransactionAsync(transaction.Id);
		updatedTransaction.Description.Should().Be(updatedTransaction.Description);

		await ShouldBeNotFoundForOthers(client => client.GetTransactionAsync(transaction.Id));
	}

	[Test]
	public async Task Transfers()
	{
		var transaction = await _client.CreateTransactionAsync(_ownerId);
		var counterpartyId = await _client.CreateCounterpartyAsync(_ownerId);
		var account1 = await _client.CreateAccountAsync(counterpartyId.Id, _ownerId);
		var account2 = await _client.CreateAccountAsync(counterpartyId.Id, _ownerId);

		var transfer = await _client.CreateTransferAsync(transaction.Id, account1.Id, account2.Id, _ownerId);

		await ShouldBeNotFoundForOthers(client => client.GetTransferAsync(transfer.Id));

		var transferCreation = transfer.ToCreation() with { BankReference = $"{transfer.BankReference}1" };

		await _otherClient.PutTransferAsync(transfer.Id, transferCreation);
		var updatedTransfer = await _client.GetTransferAsync(transfer.Id);
		updatedTransfer.BankReference.Should().Be(updatedTransfer.BankReference);

		await ShouldBeNotFoundForOthers(client => client.GetTransferAsync(transfer.Id));
		await ShouldBeNotFoundForOthers(client => client.DeleteTransferAsync(transfer.Id), true);
	}

	[Test]
	public async Task Purchases()
	{
		var transaction = await _client.CreateTransactionAsync(_ownerId);
		var unit = await _client.CreateUnitAsync(_ownerId);
		var category = await _client.CreateCategoryAsync(_ownerId);
		var product = await _client.CreateProductAsync(unit.Id, category.Id, _ownerId);

		var purchase = await _client.CreatePurchaseAsync(transaction.Id, product.Id, _ownerId);

		await ShouldBeNotFoundForOthers(client => client.GetPurchaseAsync(purchase.Id));

		var purchaseCreation = purchase.ToCreation() with { Amount = purchase.Amount + 1 };

		await _otherClient.PutPurchaseAsync(purchase.Id, purchaseCreation);
		var updatedPurchase = await _client.GetPurchaseAsync(purchase.Id);
		updatedPurchase.Amount.Should().Be(updatedPurchase.Amount);

		await ShouldBeNotFoundForOthers(client => client.GetPurchaseAsync(purchase.Id));
		await ShouldBeNotFoundForOthers(client => client.DeletePurchaseAsync(purchase.Id), true);
	}

	[Test]
	public async Task Loans()
	{
		var transaction = await _client.CreateTransactionAsync(_ownerId);
		var counterparty1 = await _client.GetMyCounterpartyAsync();
		var counterparty2 = await _otherClient.GetMyCounterpartyAsync();

		var loan = await _client.CreateLoanAsync(transaction.Id, counterparty1.Id, counterparty2.Id, _ownerId);

		await ShouldBeNotFoundForOthers(client => client.GetLoanAsync(loan.Id));

		var loanCreation = loan.ToCreation() with { Amount = loan.Amount + 1 };

		await _otherClient.PutLoanAsync(loan.Id, loanCreation);
		var updatedLoan = await _client.GetLoanAsync(loan.Id);
		updatedLoan.Amount.Should().Be(loanCreation.Amount);

		await ShouldBeNotFoundForOthers(client => client.GetLoanAsync(loan.Id));
		await ShouldBeNotFoundForOthers(client => client.DeleteLoanAsync(loan.Id), true);
	}

	[Test]
	public async Task Links()
	{
		var linkId = Guid.NewGuid();
		await _client.PutLinkAsync(linkId, new() { Uri = new("https://localhost/"), OwnerId = _ownerId });
		var link = await _client.GetLinkAsync(linkId);

		await ShouldBeNotFoundForOthers(client => client.GetLinkAsync(link.Id));

		var linkCreation = new LinkCreation { Uri = new("https://localhost/test") };

		await _otherClient.PutLinkAsync(link.Id, linkCreation);
		var updatedLink = await _client.GetLinkAsync(link.Id);
		updatedLink.Uri.Should().Be(linkCreation.Uri?.ToString());

		await ShouldBeNotFoundForOthers(client => client.GetLinkAsync(link.Id));
		await ShouldBeNotFoundForOthers(client => client.DeleteLinkAsync(link.Id), true);
	}

	private Task ShouldBeNotFoundForOthers(Func<IGnomeshadeClient, Task> func, bool inverted = false)
	{
		return ShouldReturnStatusCode(func, HttpStatusCode.NotFound, inverted);
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
