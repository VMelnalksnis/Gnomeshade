﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Accounts;
using Gnomeshade.WebApi.Models.Loans;
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
		var currency = await gnomeshadeClient.GetCurrencyAsync();
		var accountId = Guid.NewGuid();
		var account = new AccountCreation
		{
			OwnerId = ownerId,
			Name = $"{accountId:N}",
			CounterpartyId = counterpartyId,
			PreferredCurrencyId = currency.Id,
			Currencies = [new() { CurrencyId = currency.Id }],
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

	public static async Task<Category> CreateCategoryAsync(this IProductClient productClient, Guid? ownerId = null)
	{
		var categoryId = Guid.NewGuid();
		var category = new CategoryCreation { Name = $"{categoryId:N}", OwnerId = ownerId };
		await productClient.PutCategoryAsync(categoryId, category);
		return await productClient.GetCategoryAsync(categoryId);
	}

	public static async Task<DetailedTransaction> CreateDetailedTransactionAsync(
		this IGnomeshadeClient gnomeshadeClient,
		Guid account1Id,
		Guid account2Id,
		Guid productId,
		Guid issuingId,
		Guid receivingId,
		Guid? ownerId = null)
	{
		var transaction = await gnomeshadeClient.CreateTransactionAsync(ownerId);
		_ = await gnomeshadeClient.CreateTransferAsync(transaction.Id, account1Id, account2Id, ownerId);
		_ = await gnomeshadeClient.CreatePurchaseAsync(transaction.Id, productId, ownerId);
		_ = await gnomeshadeClient.CreateLoanPayment(transaction.Id, issuingId, receivingId, ownerId);

		var linkId = Guid.NewGuid();
		await gnomeshadeClient.PutLinkAsync(linkId, new() { OwnerId = ownerId, Uri = new($"https://localhost/{Guid.NewGuid().ToString()}") });
		await gnomeshadeClient.AddLinkToTransactionAsync(transaction.Id, linkId);

		return await gnomeshadeClient.GetDetailedTransactionAsync(transaction.Id);
	}

	public static async Task<Transaction> CreateTransactionAsync(
		this ITransactionClient transactionClient,
		Guid? ownerId = null)
	{
		var transactionId = Guid.NewGuid();
		var transaction = new TransactionCreation { OwnerId = ownerId };
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
			TransactionId = transactionId,
			SourceAccountId = account1.Currencies.First().Id,
			TargetAccountId = account2.Currencies.First().Id,
			SourceAmount = 10.5m,
			TargetAmount = 10.5m,
			OwnerId = ownerId,
			BookedAt = SystemClock.Instance.GetCurrentInstant(),
		};

		await gnomeshadeClient.PutTransferAsync(transferId, transfer);
		return await gnomeshadeClient.GetTransferAsync(transferId);
	}

	public static async Task<Purchase> CreatePurchaseAsync(
		this IGnomeshadeClient gnomeshadeClient,
		Guid transactionId,
		Guid productId,
		Guid? ownerId = null)
	{
		var currency = await gnomeshadeClient.GetCurrencyAsync();
		var purchaseId = Guid.NewGuid();
		var purchase = new PurchaseCreation
		{
			TransactionId = transactionId,
			CurrencyId = currency.Id,
			ProductId = productId,
			Price = 10.5m,
			Amount = 1,
			OwnerId = ownerId,
		};

		await gnomeshadeClient.PutPurchaseAsync(purchaseId, purchase);
		return await gnomeshadeClient.GetPurchaseAsync(purchaseId);
	}

	public static async Task<LoanPayment> CreateLoanPayment(
		this IGnomeshadeClient gnomeshadeClient,
		Guid transactionId,
		Guid issuingId,
		Guid receivingId,
		Guid? ownerId = null)
	{
		var loan = await gnomeshadeClient.CreateLoanAsync(issuingId, receivingId, ownerId);
		var paymentId = await gnomeshadeClient.CreateLoanPaymentAsync(new()
		{
			OwnerId = ownerId,
			LoanId = loan.Id,
			TransactionId = transactionId,
			Amount = 500m,
			Interest = 150m,
		});

		return await gnomeshadeClient.GetLoanPaymentAsync(paymentId);
	}

	public static async Task<Loan> CreateLoanAsync(this IGnomeshadeClient gnomeshadeClient, Guid? ownerId = null)
	{
		var first = await gnomeshadeClient.CreateCounterpartyAsync();
		var second = await gnomeshadeClient.CreateCounterpartyAsync();
		return await gnomeshadeClient.CreateLoanAsync(first.Id, second.Id, ownerId);
	}

	public static async Task<Loan> CreateLoanAsync(
		this IGnomeshadeClient gnomeshadeClient,
		Guid issuingId,
		Guid receivingId,
		Guid? ownerId = null)
	{
		var currency = await gnomeshadeClient.GetCurrencyAsync();

		var loanId = Guid.NewGuid();
		var loan = new LoanCreation
		{
			OwnerId = ownerId,
			Name = loanId.ToString(),
			IssuingCounterpartyId = issuingId,
			ReceivingCounterpartyId = receivingId,
			Principal = 10_000m,
			CurrencyId = currency.Id,
		};

		await gnomeshadeClient.PutLoanAsync(loanId, loan);
		return await gnomeshadeClient.GetLoanAsync(loanId);
	}

	public static async Task<Currency> GetCurrencyAsync(this IGnomeshadeClient gnomeshadeClient)
	{
		var currencies = await gnomeshadeClient.GetCurrenciesAsync();
		return currencies.First();
	}
}
