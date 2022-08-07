// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Accounts;
using Gnomeshade.WebApi.Models.Products;
using Gnomeshade.WebApi.Models.Transactions;

using NodaTime;

namespace Gnomeshade.TestingHelpers.Models;

public static class GnomeshadeClientExtensions
{
	public static async Task<Account> CreateAccountAsync(
		this IGnomeshadeClient gnomeshadeClient,
		Guid counterpartyId,
		Guid? ownerId = null)
	{
		var currency = (await gnomeshadeClient.GetCurrenciesAsync()).First();
		var accountId = Guid.NewGuid();
		var account = new AccountCreation
		{
			OwnerId = ownerId,
			Name = $"{accountId:N}",
			CounterpartyId = counterpartyId,
			PreferredCurrencyId = currency.Id,
			Currencies = new() { new() { CurrencyId = currency.Id } },
		};

		await gnomeshadeClient.PutAccountAsync(accountId, account);
		return await gnomeshadeClient.GetAccountAsync(accountId);
	}

	public static async Task<Unit> CreateUnitAsync(this IGnomeshadeClient gnomeshadeClient, Guid? ownerId = null)
	{
		var unitId = Guid.NewGuid();
		var unit = new UnitCreation { Name = $"{unitId:N}", OwnerId = ownerId };
		await gnomeshadeClient.PutUnitAsync(unitId, unit);
		return await gnomeshadeClient.GetUnitAsync(unitId);
	}

	public static async Task<Product> CreateProductAsync(
		this IGnomeshadeClient gnomeshadeClient,
		Guid unitId,
		Guid categoryId,
		Guid? ownerId = null)
	{
		var productId = Guid.NewGuid();
		var product = new ProductCreation
		{
			Name = $"{productId:N}",
			UnitId = unitId,
			CategoryId = categoryId,
			OwnerId = ownerId,
		};
		await gnomeshadeClient.PutProductAsync(productId, product);
		return await gnomeshadeClient.GetProductAsync(productId);
	}

	public static async Task<Counterparty> CreateCounterpartyAsync(
		this IGnomeshadeClient accountClient,
		Guid? ownerId = null)
	{
		var counterpartyId = Guid.NewGuid();
		var counterparty = new CounterpartyCreation { Name = $"{counterpartyId:N}", OwnerId = ownerId };
		await accountClient.PutCounterpartyAsync(counterpartyId, counterparty);
		return await accountClient.GetCounterpartyAsync(counterpartyId);
	}

	public static async Task<Category> CreateCategoryAsync(this IGnomeshadeClient productClient, Guid? ownerId = null)
	{
		var categoryId = Guid.NewGuid();
		var category = new CategoryCreation { Name = $"{categoryId:N}", OwnerId = ownerId };
		await productClient.PutCategoryAsync(categoryId, category);
		return await productClient.GetCategoryAsync(categoryId);
	}

	public static async Task<Transaction> CreateTransactionAsync(
		this ITransactionClient transactionClient,
		Guid? ownerId = null)
	{
		var transactionId = Guid.NewGuid();
		var transaction = new TransactionCreation
		{
			BookedAt = SystemClock.Instance.GetCurrentInstant(),
			OwnerId = ownerId,
		};
		await transactionClient.PutTransactionAsync(transactionId, transaction);
		return await transactionClient.GetTransactionAsync(transactionId);
	}

	public static async Task<Transfer> CreateTransferAsync(
		this IGnomeshadeClient gnomeshadeClient,
		Guid transactionId,
		Guid account1Id,
		Guid account2Id,
		Guid? ownerId = null)
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
			OwnerId = ownerId,
		};

		await gnomeshadeClient.PutTransferAsync(transactionId, transferId, transfer);
		return await gnomeshadeClient.GetTransferAsync(transactionId, transferId);
	}

	public static async Task<Purchase> CreatePurchaseAsync(
		this IGnomeshadeClient gnomeshadeClient,
		Guid transactionId,
		Guid productId,
		Guid? ownerId = null)
	{
		var currency = (await gnomeshadeClient.GetCurrenciesAsync()).First();
		var purchaseId = Guid.NewGuid();
		var purchase = new PurchaseCreation
		{
			CurrencyId = currency.Id,
			ProductId = productId,
			Price = 10.5m,
			Amount = 1,
			OwnerId = ownerId,
		};

		await gnomeshadeClient.PutPurchaseAsync(transactionId, purchaseId, purchase);
		return await gnomeshadeClient.GetPurchaseAsync(transactionId, purchaseId);
	}

	public static async Task<Loan> CreateLoanAsync(
		this IGnomeshadeClient gnomeshadeClient,
		Guid transactionId,
		Guid issuingId,
		Guid receivingId,
		Guid? ownerId = null)
	{
		var currency = (await gnomeshadeClient.GetCurrenciesAsync()).First();
		var loanId = Guid.NewGuid();
		var purchase = new LoanCreation
		{
			CurrencyId = currency.Id,
			Amount = 1,
			IssuingCounterpartyId = issuingId,
			ReceivingCounterpartyId = receivingId,
			OwnerId = ownerId,
		};

		await gnomeshadeClient.PutLoanAsync(transactionId, loanId, purchase);
		return await gnomeshadeClient.GetLoanAsync(transactionId, loanId);
	}
}
