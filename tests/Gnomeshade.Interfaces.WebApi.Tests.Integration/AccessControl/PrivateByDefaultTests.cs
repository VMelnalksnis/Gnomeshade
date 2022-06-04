// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Accounts;
using Gnomeshade.Interfaces.WebApi.Models.Products;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;

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
		var counterpartyId = await CreateCounterpartyAsync(_client);

		await ShouldReturnNotFoundForOtherUsers(
			(client, id) => client.GetCounterpartyAsync(id),
			counterpartyId);
	}

	[Test]
	public async Task Accounts()
	{
		var counterpartyId = await CreateCounterpartyAsync(_client);
		var accountId = await CreateAccountAsync(_client, counterpartyId);

		await ShouldReturnNotFoundForOtherUsers(
			(client, id) => client.GetAccountAsync(id),
			accountId);
	}

	[Test]
	public async Task Categories()
	{
		var categoryId = await CreateCategoryAsync(_client);

		await ShouldReturnNotFoundForOtherUsers(
			(client, id) => client.GetCategoryAsync(id),
			categoryId);
	}

	[Test]
	public async Task Units()
	{
		var unitId = await CreateUnitAsync(_client);

		await ShouldReturnNotFoundForOtherUsers(
			(client, id) => client.GetUnitAsync(id),
			unitId);
	}

	[Test]
	public async Task Products()
	{
		var unitId = await CreateUnitAsync(_client);
		var categoryId = await CreateCategoryAsync(_client);
		var productId = await CreateProductAsync(_client, unitId, categoryId);

		await ShouldReturnNotFoundForOtherUsers(
			(client, id) => client.GetProductAsync(id),
			productId);
	}

	[Test]
	public async Task Transactions()
	{
		var transactionId = await CreateTransactionAsync(_client);

		await ShouldReturnNotFoundForOtherUsers(
			(client, id) => client.GetTransactionAsync(id),
			transactionId);
	}

	[Test]
	public async Task Transfers()
	{
		var transactionId = await CreateTransactionAsync(_client);
		var counterpartyId = await CreateCounterpartyAsync(_client);
		var account1Id = await CreateAccountAsync(_client, counterpartyId);
		var account2Id = await CreateAccountAsync(_client, counterpartyId);

		var transferId = await CreateTransferAsync(_client, transactionId, account1Id, account2Id);

		await ShouldReturnNotFoundForOtherUsers(
			(client, id, otherId) => client.GetTransferAsync(id, otherId),
			transactionId,
			transferId);
	}

	[Test]
	public async Task Purchases()
	{
		var transactionId = await CreateTransactionAsync(_client);
		var unitId = await CreateUnitAsync(_client);
		var categoryId = await CreateCategoryAsync(_client);
		var productId = await CreateProductAsync(_client, unitId, categoryId);

		var purchaseId = await CreatePurchaseAsync(_client, transactionId, productId);

		await ShouldReturnNotFoundForOtherUsers(
			(client, id, otherId) => client.GetPurchaseAsync(id, otherId),
			transactionId,
			purchaseId);
	}

	private static async Task<Guid> CreateCounterpartyAsync(IAccountClient accountClient)
	{
		var counterpartyId = Guid.NewGuid();
		var counterparty = new CounterpartyCreationModel { Name = $"{counterpartyId:N}" };
		await accountClient.PutCounterpartyAsync(counterpartyId, counterparty);
		return counterpartyId;
	}

	private static async Task<Guid> CreateAccountAsync(IAccountClient accountClient, Guid counterpartyId)
	{
		var currency = (await accountClient.GetCurrenciesAsync()).First();
		var accountId = Guid.NewGuid();
		var account = new AccountCreationModel
		{
			Name = $"{accountId:N}",
			CounterpartyId = counterpartyId,
			PreferredCurrencyId = currency.Id,
			Currencies = new() { new() { CurrencyId = currency.Id } },
		};

		await accountClient.PutAccountAsync(accountId, account);
		return accountId;
	}

	private static async Task<Guid> CreateCategoryAsync(IProductClient productClient)
	{
		var categoryId = Guid.NewGuid();
		var category = new CategoryCreation { Name = $"{categoryId:N}" };
		await productClient.PutCategoryAsync(categoryId, category);
		return categoryId;
	}

	private static async Task<Guid> CreateUnitAsync(IProductClient productClient)
	{
		var unitId = Guid.NewGuid();
		var unit = new UnitCreationModel { Name = $"{unitId:N}" };
		await productClient.PutUnitAsync(unitId, unit);
		return unitId;
	}

	private static async Task<Guid> CreateProductAsync(IProductClient productClient, Guid unitId, Guid categoryId)
	{
		var productId = Guid.NewGuid();
		var product = new ProductCreationModel { Name = $"{productId:N}", UnitId = unitId, CategoryId = categoryId };
		await productClient.PutProductAsync(productId, product);
		return productId;
	}

	private static async Task<Guid> CreateTransactionAsync(ITransactionClient transactionClient)
	{
		var transactionId = Guid.NewGuid();
		var transaction = new TransactionCreationModel { BookedAt = SystemClock.Instance.GetCurrentInstant() };
		await transactionClient.PutTransactionAsync(transactionId, transaction);
		return transactionId;
	}

	private static async Task<Guid> CreateTransferAsync(
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
		return transferId;
	}

	private static async Task<Guid> CreatePurchaseAsync(
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
		return purchaseId;
	}

	private async Task ShouldReturnNotFoundForOtherUsers(Func<IGnomeshadeClient, Guid, Task> func, Guid id)
	{
		using (new AssertionScope())
		{
			await FluentActions
				.Awaiting(() => func(_client, id))
				.Should()
				.NotThrowAsync();

			(await FluentActions
					.Awaiting(() => func(_otherClient, id))
					.Should()
					.ThrowAsync<HttpRequestException>())
				.Which.StatusCode.Should()
				.Be(HttpStatusCode.NotFound);
		}
	}

	private async Task ShouldReturnNotFoundForOtherUsers(
		Func<IGnomeshadeClient, Guid, Guid, Task> func,
		Guid id,
		Guid otherId)
	{
		using (new AssertionScope())
		{
			await FluentActions
				.Awaiting(() => func(_client, id, otherId))
				.Should()
				.NotThrowAsync();

			(await FluentActions
					.Awaiting(() => func(_otherClient, id, otherId))
					.Should()
					.ThrowAsync<HttpRequestException>())
				.Which.StatusCode.Should()
				.Be(HttpStatusCode.NotFound);
		}
	}
}
