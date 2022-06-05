// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Accounts;
using Gnomeshade.Interfaces.WebApi.Models.Products;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;

using NodaTime;

namespace Gnomeshade.TestingHelpers.Models;

public static class GnomeshadeClientExtensions
{
	public static async Task<Account> CreateAccountAsync(this IGnomeshadeClient gnomeshadeClient, Guid counterpartyId)
	{
		var currency = (await gnomeshadeClient.GetCurrenciesAsync()).First();
		var accountId = Guid.NewGuid();
		var account = new AccountCreation
		{
			Name = $"{accountId:N}",
			CounterpartyId = counterpartyId,
			PreferredCurrencyId = currency.Id,
			Currencies = new() { new() { CurrencyId = currency.Id } },
		};

		await gnomeshadeClient.PutAccountAsync(accountId, account);
		return await gnomeshadeClient.GetAccountAsync(accountId);
	}

	public static async Task<Unit> CreateUnitAsync(this IGnomeshadeClient gnomeshadeClient)
	{
		var unitId = Guid.NewGuid();
		var unit = new UnitCreation { Name = $"{unitId:N}" };
		await gnomeshadeClient.PutUnitAsync(unitId, unit);
		return await gnomeshadeClient.GetUnitAsync(unitId);
	}

	public static async Task<Product> CreateProductAsync(
		this IGnomeshadeClient gnomeshadeClient,
		Guid unitId,
		Guid categoryId)
	{
		var productId = Guid.NewGuid();
		var product = new ProductCreation { Name = $"{productId:N}", UnitId = unitId, CategoryId = categoryId };
		await gnomeshadeClient.PutProductAsync(productId, product);
		return await gnomeshadeClient.GetProductAsync(productId);
	}

	public static async Task<Counterparty> CreateCounterpartyAsync(this IGnomeshadeClient accountClient)
	{
		var counterpartyId = Guid.NewGuid();
		var counterparty = new CounterpartyCreation { Name = $"{counterpartyId:N}" };
		await accountClient.PutCounterpartyAsync(counterpartyId, counterparty);
		return await accountClient.GetCounterpartyAsync(counterpartyId);
	}

	public static async Task<Category> CreateCategoryAsync(this IGnomeshadeClient productClient)
	{
		var categoryId = Guid.NewGuid();
		var category = new CategoryCreation { Name = $"{categoryId:N}" };
		await productClient.PutCategoryAsync(categoryId, category);
		return await productClient.GetCategoryAsync(categoryId);
	}

	public static async Task<Transaction> CreateTransactionAsync(this ITransactionClient transactionClient)
	{
		var transactionId = Guid.NewGuid();
		var transaction = new TransactionCreation { BookedAt = SystemClock.Instance.GetCurrentInstant() };
		await transactionClient.PutTransactionAsync(transactionId, transaction);
		return await transactionClient.GetTransactionAsync(transactionId);
	}

	public static async Task<Transfer> CreateTransferAsync(
		this IGnomeshadeClient gnomeshadeClient,
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

	public static async Task<Purchase> CreatePurchaseAsync(
		this IGnomeshadeClient gnomeshadeClient,
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

	public static async Task<Loan> CreateLoanAsync(
		this IGnomeshadeClient gnomeshadeClient,
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
}
