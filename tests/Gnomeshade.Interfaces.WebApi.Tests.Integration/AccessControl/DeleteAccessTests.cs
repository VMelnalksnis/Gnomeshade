// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Owners;
using Gnomeshade.TestingHelpers.Models;

using NodaTime;

namespace Gnomeshade.Interfaces.WebApi.Tests.Integration.AccessControl;

public sealed class DeleteAccessTests
{
	private readonly Guid _deleteOwnershipId = Guid.NewGuid();

	private IGnomeshadeClient _client = null!;
	private IGnomeshadeClient _otherClient = null!;

	[OneTimeSetUp]
	public async Task OneTimeSetUp()
	{
		_client = await WebserverSetup.CreateAuthorizedClientAsync();
		_otherClient = await WebserverSetup.CreateAuthorizedSecondClientAsync();

		var counterparty = await _client.GetMyCounterpartyAsync();
		var otherCounterparty = await _otherClient.GetMyCounterpartyAsync();
		var accesses = await _client.GetAccessesAsync();
		var deleteAccess = accesses.Single(access => access.Name == "Delete");
		var owners = await _client.GetOwnersAsync();
		var deleteOwnership = new OwnershipCreation
		{
			AccessId = deleteAccess.Id,
			OwnerId = owners.Single(owner => owner.Id == counterparty.OwnerId).Id,
			UserId = otherCounterparty.OwnerId,
		};

		await _client.PutOwnershipAsync(_deleteOwnershipId, deleteOwnership);
	}

	[OneTimeTearDown]
	public async Task OneTimeTearDown()
	{
		await _client.DeleteOwnershipAsync(_deleteOwnershipId);
	}

	[Test]
	public async Task Categories()
	{
		var category = await _client.CreateCategoryAsync();

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
	}

	[Test]
	public async Task Transactions()
	{
		var transaction = await _client.CreateTransactionAsync();

		await ShouldBeNotFoundForOthers(client => client.GetTransactionAsync(transaction.Id));

		var updatedTransaction = transaction.ToCreation() with { ValuedAt = SystemClock.Instance.GetCurrentInstant() };

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
