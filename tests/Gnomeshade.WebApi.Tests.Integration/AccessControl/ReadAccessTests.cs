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
using Gnomeshade.WebApi.Models.Projects;
using Gnomeshade.WebApi.Tests.Integration.Fixtures;

namespace Gnomeshade.WebApi.Tests.Integration.AccessControl;

[TestFixtureSource(typeof(OwnerTestFixtureSource))]
public sealed class ReadAccessTests : WebserverTests
{
	private readonly Func<IGnomeshadeClient, Task<Guid>> _ownerIdFunc;
	private readonly Guid _readOwnershipId = Guid.NewGuid();

	private IGnomeshadeClient _client = null!;
	private IGnomeshadeClient _otherClient = null!;
	private Guid? _ownerId;

	public ReadAccessTests(Func<IGnomeshadeClient, Task<Guid>> ownerIdFunc, WebserverFixture fixture)
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
		var readAccess = accesses.Single(access => access.Name == "Read");
		var ownerId = await _ownerIdFunc(_client);
		var readOwnership = new OwnershipCreation
		{
			AccessId = readAccess.Id,
			OwnerId = ownerId,
			UserId = otherCounterparty.OwnerId,
		};

		await _client.PutOwnershipAsync(_readOwnershipId, readOwnership);

		_ownerId = ownerId == counterparty.OwnerId ? null : ownerId;
	}

	[OneTimeTearDown]
	public async Task OneTimeTearDown()
	{
		await _client.DeleteOwnershipAsync(_readOwnershipId);
	}

	[Test]
	public async Task Counterparties()
	{
		var counterparty = await _client.CreateCounterpartyAsync(_ownerId);

		await ShouldReturnTheSame(client => client.GetCounterpartyAsync(counterparty.Id));

		var counterpartyCreation = new CounterpartyCreation { Name = $"{counterparty.Name}1" };

		await ShouldBeForbiddenForOthers(client => client.PutCounterpartyAsync(counterparty.Id, counterpartyCreation));
		await ShouldReturnTheSame(client => client.GetCounterpartyAsync(counterparty.Id));
	}

	[Test]
	public async Task Accounts()
	{
		var counterpartyId = await _client.CreateCounterpartyAsync(_ownerId);
		var account = await _client.CreateAccountAsync(counterpartyId.Id, _ownerId);

		await ShouldReturnTheSame(client => client.GetAccountAsync(account.Id));

		var accountCreation = account.ToCreation() with { Name = $"{account.Name}1" };

		await ShouldBeForbiddenForOthers(client => client.PutAccountAsync(account.Id, accountCreation));
		await ShouldReturnTheSame(client => client.GetAccountAsync(account.Id));
	}

	[Test]
	public async Task Categories()
	{
		var category = await _client.CreateCategoryAsync(_ownerId);

		await ShouldReturnTheSame(client => client.GetCategoryAsync(category.Id));

		var updatedCategory = category.ToCreation() with { Name = $"{category.Name}1" };

		await ShouldBeForbiddenForOthers(client => client.PutCategoryAsync(category.Id, updatedCategory));
		await ShouldReturnTheSame(client => client.GetCategoryAsync(category.Id));
		await ShouldBeNotFoundForOthers(client => client.DeleteCategoryAsync(category.Id));
	}

	[Test]
	public async Task Units()
	{
		var unit = await _client.CreateUnitAsync(_ownerId);

		await ShouldReturnTheSame(client => client.GetUnitAsync(unit.Id));

		var updatedUnit = unit.ToCreation() with { Name = $"{unit.Name}1" };

		await ShouldBeForbiddenForOthers(client => client.PutUnitAsync(unit.Id, updatedUnit));
		await ShouldReturnTheSame(client => client.GetUnitAsync(unit.Id));
	}

	[Test]
	public async Task Products()
	{
		var unit = await _client.CreateUnitAsync();
		var category = await _client.CreateCategoryAsync(_ownerId);
		var product = await _client.CreateProductAsync(unit.Id, category.Id, _ownerId);

		await ShouldReturnTheSame(client => client.GetProductAsync(product.Id));

		var updatedProduct = product.ToCreation() with { Name = $"{category.Name}1" };

		await ShouldBeForbiddenForOthers(client => client.PutProductAsync(category.Id, updatedProduct));
		await ShouldReturnTheSame(client => client.GetProductAsync(product.Id));
	}

	[Test]
	public async Task Transactions()
	{
		var transaction = await _client.CreateTransactionAsync(_ownerId);

		await ShouldReturnTheSame(client => client.GetTransactionAsync(transaction.Id));

		var updatedTransaction = transaction.ToCreation() with { Description = Guid.NewGuid().ToString() };

		await ShouldBeForbiddenForOthers(client => client.PutTransactionAsync(transaction.Id, updatedTransaction));
		await ShouldReturnTheSame(client => client.GetTransactionAsync(transaction.Id));
		await ShouldBeNotFoundForOthers(client => client.DeleteTransactionAsync(transaction.Id));
	}

	[Test]
	public async Task Transfers()
	{
		var transaction = await _client.CreateTransactionAsync(_ownerId);
		var counterparty = await _client.CreateCounterpartyAsync(_ownerId);
		var account1 = await _client.CreateAccountAsync(counterparty.Id, _ownerId);
		var account2 = await _client.CreateAccountAsync(counterparty.Id, _ownerId);

		var transfer = await _client.CreateTransferAsync(transaction.Id, account1.Id, account2.Id, _ownerId);

		await ShouldReturnTheSame(client => client.GetTransferAsync(transfer.Id));

		var updatedTransfer = transfer.ToCreation() with { BankReference = $"{transfer.BankReference}1" };

		await ShouldBeForbiddenForOthers(client => client.PutTransferAsync(transfer.Id, updatedTransfer));
		await ShouldReturnTheSame(client => client.GetTransferAsync(transfer.Id));
		await ShouldBeNotFoundForOthers(client => client.DeleteTransferAsync(transfer.Id), true);
	}

	[Test]
	public async Task Purchases()
	{
		var transaction = await _client.CreateTransactionAsync(_ownerId);
		var unit = await _client.CreateUnitAsync();
		var category = await _client.CreateCategoryAsync(_ownerId);
		var product = await _client.CreateProductAsync(unit.Id, category.Id, _ownerId);

		var purchase = await _client.CreatePurchaseAsync(transaction.Id, product.Id, _ownerId);

		await ShouldReturnTheSame(client => client.GetPurchaseAsync(purchase.Id));

		var updatedTransfer = purchase.ToCreation() with { Amount = purchase.Amount + 1 };

		await ShouldBeForbiddenForOthers(client => client.PutPurchaseAsync(purchase.Id, updatedTransfer));
		await ShouldReturnTheSame(client => client.GetPurchaseAsync(purchase.Id));
		await ShouldBeNotFoundForOthers(client => client.DeletePurchaseAsync(purchase.Id), true);
	}

	[Test]
	public async Task Loans()
	{
		var loan = await _client.CreateLoanAsync(_ownerId);

		await ShouldReturnTheSame(client => client.GetLoanAsync(loan.Id));

		var updatedLoan = loan.ToCreation() with { Principal = loan.Principal + 10 };

		await ShouldBeForbiddenForOthers(client => client.PutLoanAsync(loan.Id, updatedLoan));
		await ShouldReturnTheSame(client => client.GetLoanAsync(loan.Id));
		await ShouldBeNotFoundForOthers(client => client.DeleteLoanAsync(loan.Id), true);
	}

	[Test]
	public async Task LoanPayments()
	{
		var transaction = await _client.CreateTransactionAsync(_ownerId);
		var counterparty1 = await _client.GetMyCounterpartyAsync();
		var counterparty2 = await _otherClient.GetMyCounterpartyAsync();

		var loanPayment = await _client.CreateLoanPayment(transaction.Id, counterparty1.Id, counterparty2.Id, _ownerId);

		await ShouldReturnTheSame(client => client.GetLoanPaymentAsync(loanPayment.Id));

		var updatedLoan = loanPayment.ToCreation() with { Amount = loanPayment.Amount + 1 };

		await ShouldBeForbiddenForOthers(client => client.PutLoanPaymentAsync(loanPayment.Id, updatedLoan));
		await ShouldReturnTheSame(client => client.GetLoanPaymentAsync(loanPayment.Id));
		await ShouldBeNotFoundForOthers(client => client.DeleteLoanPaymentAsync(loanPayment.Id), true);
	}

	[Test]
	public async Task Links()
	{
		var linkId = Guid.NewGuid();
		await _client.PutLinkAsync(linkId, new() { Uri = new("https://localhost/"), OwnerId = _ownerId });
		var link = await _client.GetLinkAsync(linkId);

		await ShouldReturnTheSame(client => client.GetLinkAsync(link.Id));

		var updatedLink = new LinkCreation { Uri = new("https://localhost/test"), OwnerId = _ownerId };

		await ShouldBeForbiddenForOthers(client => client.PutLinkAsync(link.Id, updatedLink));
		await ShouldReturnTheSame(client => client.GetLinkAsync(link.Id));
		await ShouldBeNotFoundForOthers(client => client.DeleteLinkAsync(link.Id), true);
	}

	[Test]
	public async Task Projects()
	{
		var projectId = await _client.CreateProjectAsync(new()
		{
			Name = Guid.NewGuid().ToString(),
			OwnerId = _ownerId,
		});

		await ShouldReturnTheSame(client => client.GetProjectAsync(projectId));

		var updatedProject = new ProjectCreation { Name = Guid.NewGuid().ToString(), OwnerId = _ownerId };

		await ShouldBeForbiddenForOthers(client => client.PutProjectAsync(projectId, updatedProject));
		await ShouldReturnTheSame(client => client.GetProjectAsync(projectId));
		await ShouldBeNotFoundForOthers(client => client.DeleteProjectAsync(projectId), true);
	}

	private async Task ShouldReturnTheSame<TResult>(Func<IGnomeshadeClient, Task<TResult>> func)
		where TResult : class
	{
		var result = await func(_client);
		var otherResult = await func(_otherClient);

		result.Should().BeEquivalentTo(otherResult);
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
