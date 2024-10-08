﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Entities.Abstractions;
using Gnomeshade.TestingHelpers.Models;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models;
using Gnomeshade.WebApi.Models.Owners;
using Gnomeshade.WebApi.Models.Projects;
using Gnomeshade.WebApi.Tests.Integration.Fixtures;

namespace Gnomeshade.WebApi.Tests.Integration.AccessControl;

[TestFixtureSource(typeof(OwnerTestFixtureSource))]
public sealed class DeleteAccessTests : WebserverTests
{
	private readonly Func<IGnomeshadeClient, Task<Guid>> _ownerIdFunc;
	private readonly Guid _deleteOwnershipId = Guid.NewGuid();

	private IGnomeshadeClient _client = null!;
	private IGnomeshadeClient _otherClient = null!;
	private Guid? _ownerId;

	public DeleteAccessTests(Func<IGnomeshadeClient, Task<Guid>> ownerIdFunc, WebserverFixture fixture)
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
		var deleteAccess = accesses.Single(access => access.Name == "Delete");
		var ownerId = await _ownerIdFunc(_client);
		var deleteOwnership = new OwnershipCreation
		{
			AccessId = deleteAccess.Id,
			OwnerId = ownerId,
			UserId = otherCounterparty.OwnerId,
		};

		await _client.PutOwnershipAsync(_deleteOwnershipId, deleteOwnership);

		_ownerId = ownerId == counterparty.OwnerId ? null : ownerId;
	}

	[OneTimeTearDown]
	public async Task OneTimeTearDown()
	{
		await _client.DeleteOwnershipAsync(_deleteOwnershipId);
	}

	[Test]
	public async Task Categories()
	{
		var category = await _client.CreateCategoryAsync(_ownerId);

		await ShouldBeNotFoundForOthers(client => client.GetCategoryAsync(category.Id));

		var updatedCategory = category.ToCreation() with { Name = $"{category.Name}1" };

		await ShouldBeForbiddenForOthers(client => client.PutCategoryAsync(category.Id, updatedCategory));
		await ShouldBeNotFoundForOthers(client => client.GetCategoryAsync(category.Id));

		await FluentActions
			.Awaiting(() => _otherClient.DeleteCategoryAsync(category.Id))
			.Should()
			.NotThrowAsync();

		(await FluentActions
				.Awaiting(() => _client.GetCategoryAsync(category.Id))
				.Should()
				.ThrowAsync<HttpRequestException>())
			.Which.StatusCode.Should()
			.Be(HttpStatusCode.NotFound);

		await ShouldBeDeletedWithClient<CategoryEntity>(category.Id, _otherClient);
	}

	[Test]
	public async Task Transactions()
	{
		var transaction = await _client.CreateTransactionAsync(_ownerId);

		await ShouldBeNotFoundForOthers(client => client.GetTransactionAsync(transaction.Id));

		var updatedTransaction = transaction.ToCreation() with { Description = Guid.NewGuid().ToString() };

		await ShouldBeForbiddenForOthers(client => client.PutTransactionAsync(transaction.Id, updatedTransaction));
		await ShouldBeNotFoundForOthers(client => client.GetTransactionAsync(transaction.Id));

		await FluentActions
			.Awaiting(() => _otherClient.DeleteTransactionAsync(transaction.Id))
			.Should()
			.NotThrowAsync();

		(await FluentActions
				.Awaiting(() => _client.GetTransactionAsync(transaction.Id))
				.Should()
				.ThrowAsync<HttpRequestException>())
			.Which.StatusCode.Should()
			.Be(HttpStatusCode.NotFound);

		await ShouldBeDeletedWithClient<TransactionEntity>(transaction.Id, _otherClient);
	}

	[Test]
	public async Task Transfers()
	{
		var transaction = await _client.CreateTransactionAsync(_ownerId);
		var counterparty = await _client.CreateCounterpartyAsync(_ownerId);
		var account1 = await _client.CreateAccountAsync(counterparty.Id, _ownerId);
		var account2 = await _client.CreateAccountAsync(counterparty.Id, _ownerId);

		var transfer = await _client.CreateTransferAsync(transaction.Id, account1.Id, account2.Id, _ownerId);

		await ShouldBeNotFoundForOthers(client => client.GetTransferAsync(transfer.Id));

		var updatedTransfer = transfer.ToCreation() with { BankReference = $"{transfer.BankReference}1" };

		await ShouldBeForbiddenForOthers(client => client.PutTransferAsync(transfer.Id, updatedTransfer));
		await ShouldBeNotFoundForOthers(client => client.GetTransferAsync(transfer.Id));

		await FluentActions
			.Awaiting(() => _otherClient.DeleteTransferAsync(transfer.Id))
			.Should()
			.NotThrowAsync();

		(await FluentActions
				.Awaiting(() => _client.GetTransferAsync(transfer.Id))
				.Should()
				.ThrowAsync<HttpRequestException>())
			.Which.StatusCode.Should()
			.Be(HttpStatusCode.NotFound);

		await ShouldBeDeletedWithClient<TransferEntity>(transfer.Id, _otherClient);
	}

	[Test]
	public async Task Purchases()
	{
		var transaction = await _client.CreateTransactionAsync(_ownerId);
		var unit = await _client.CreateUnitAsync();
		var category = await _client.CreateCategoryAsync(_ownerId);
		var product = await _client.CreateProductAsync(unit.Id, category.Id, _ownerId);

		var purchase = await _client.CreatePurchaseAsync(transaction.Id, product.Id, _ownerId);

		await ShouldBeNotFoundForOthers(client => client.GetPurchaseAsync(purchase.Id));

		var updatedTransfer = purchase.ToCreation() with { Amount = purchase.Amount + 1 };

		await ShouldBeForbiddenForOthers(client => client.PutPurchaseAsync(purchase.Id, updatedTransfer));
		await ShouldBeNotFoundForOthers(client => client.GetPurchaseAsync(purchase.Id));

		await FluentActions
			.Awaiting(() => _otherClient.DeletePurchaseAsync(purchase.Id))
			.Should()
			.NotThrowAsync();

		(await FluentActions
				.Awaiting(() => _client.GetPurchaseAsync(purchase.Id))
				.Should()
				.ThrowAsync<HttpRequestException>())
			.Which.StatusCode.Should()
			.Be(HttpStatusCode.NotFound);

		await ShouldBeDeletedWithClient<PurchaseEntity>(purchase.Id, _otherClient);
	}

	[Test]
	public async Task Loans()
	{
		var loan = await _client.CreateLoanAsync(_ownerId);

		await ShouldBeNotFoundForOthers(client => client.GetLoanAsync(loan.Id));

		var updatedLoan = loan.ToCreation() with { Principal = loan.Principal + 1 };

		await ShouldBeForbiddenForOthers(client => client.PutLoanAsync(loan.Id, updatedLoan));
		await ShouldBeNotFoundForOthers(client => client.GetLoanAsync(loan.Id));

		await FluentActions
			.Awaiting(() => _otherClient.DeleteLoanAsync(loan.Id))
			.Should()
			.NotThrowAsync();

		(await FluentActions
				.Awaiting(() => _client.GetLoanAsync(loan.Id))
				.Should()
				.ThrowAsync<HttpRequestException>())
			.Which.StatusCode.Should()
			.Be(HttpStatusCode.NotFound);

		await ShouldBeDeletedWithClient<Loan2Entity>(loan.Id, _otherClient);
	}

	[Test]
	public async Task LoanPayments()
	{
		var transaction = await _client.CreateTransactionAsync(_ownerId);
		var counterparty1 = await _client.GetMyCounterpartyAsync();
		var counterparty2 = await _otherClient.GetMyCounterpartyAsync();

		var loanPayment = await _client.CreateLoanPayment(transaction.Id, counterparty1.Id, counterparty2.Id, _ownerId);

		await ShouldBeNotFoundForOthers(client => client.GetLoanPaymentAsync(loanPayment.Id));

		var updatedLoan = loanPayment.ToCreation() with { Amount = loanPayment.Amount + 1 };

		await ShouldBeForbiddenForOthers(client => client.PutLoanPaymentAsync(loanPayment.Id, updatedLoan));
		await ShouldBeNotFoundForOthers(client => client.GetLoanPaymentAsync(loanPayment.Id));

		await FluentActions
			.Awaiting(() => _otherClient.DeleteLoanPaymentAsync(loanPayment.Id))
			.Should()
			.NotThrowAsync();

		(await FluentActions
				.Awaiting(() => _client.GetLoanPaymentAsync(loanPayment.Id))
				.Should()
				.ThrowAsync<HttpRequestException>())
			.Which.StatusCode.Should()
			.Be(HttpStatusCode.NotFound);

		await ShouldBeDeletedWithClient<LoanPaymentEntity>(loanPayment.Id, _otherClient);
	}

	[Test]
	public async Task Links()
	{
		var linkId = Guid.NewGuid();
		await _client.PutLinkAsync(linkId, new() { Uri = new("https://localhost/"), OwnerId = _ownerId });
		var link = await _client.GetLinkAsync(linkId);

		await ShouldBeNotFoundForOthers(client => client.GetLinkAsync(link.Id));

		var updatedLink = new LinkCreation { Uri = new("https://localhost/test"), OwnerId = _ownerId };

		await ShouldBeForbiddenForOthers(client => client.PutLinkAsync(link.Id, updatedLink));
		await ShouldBeNotFoundForOthers(client => client.GetLinkAsync(link.Id));

		await FluentActions
			.Awaiting(() => _otherClient.DeleteLinkAsync(link.Id))
			.Should()
			.NotThrowAsync();

		(await FluentActions
				.Awaiting(() => _client.GetLinkAsync(link.Id))
				.Should()
				.ThrowAsync<HttpRequestException>())
			.Which.StatusCode.Should()
			.Be(HttpStatusCode.NotFound);

		await ShouldBeDeletedWithClient<LinkEntity>(link.Id, _otherClient);
	}

	[Test]
	public async Task Projects()
	{
		var projectId = await _client.CreateProjectAsync(new() { Name = Guid.NewGuid().ToString(), OwnerId = _ownerId });

		await ShouldBeNotFoundForOthers(client => client.GetProjectAsync(projectId));

		var updatedProject = new ProjectCreation { Name = Guid.NewGuid().ToString(), OwnerId = _ownerId };

		await ShouldBeForbiddenForOthers(client => client.PutProjectAsync(projectId, updatedProject));
		await ShouldBeNotFoundForOthers(client => client.GetProjectAsync(projectId));

		await FluentActions
			.Awaiting(() => _otherClient.DeleteProjectAsync(projectId))
			.Should()
			.NotThrowAsync();

		(await FluentActions
				.Awaiting(() => _client.GetProjectAsync(projectId))
				.Should()
				.ThrowAsync<HttpRequestException>())
			.Which.StatusCode.Should()
			.Be(HttpStatusCode.NotFound);

		await ShouldBeDeletedWithClient<ProjectEntity>(projectId, _otherClient);
	}

	private async Task ShouldBeDeletedWithClient<TEntity>(Guid id, IGnomeshadeClient client)
		where TEntity : Entity
	{
		using var scope = Fixture.CreateScope();
		var deletedEntity = await Fixture.GetEntityRepository(scope).FindByIdAsync<TEntity>(id);
		var userId = (await client.GetMyCounterpartyAsync()).CreatedByUserId;

		using var assertionScope = new AssertionScope();

		deletedEntity.Should().NotBeNull();
		deletedEntity?.DeletedAt.Should().NotBeNull();
		deletedEntity?.DeletedByUserId.Should().Be(userId);
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
