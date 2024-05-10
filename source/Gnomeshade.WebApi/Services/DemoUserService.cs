// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Accounts;
using Gnomeshade.WebApi.Models.Loans;
using Gnomeshade.WebApi.Models.Products;

using NodaTime;

namespace Gnomeshade.WebApi.Services;

internal sealed class DemoUserService
{
	private readonly IGnomeshadeClient _client;
	private readonly IClock _clock;
	private readonly IDateTimeZoneProvider _zoneProvider;
	private readonly Random _random = new(1658746);

	public DemoUserService(IGnomeshadeClient gnomeshadeClient, IClock clock, IDateTimeZoneProvider zoneProvider)
	{
		_client = gnomeshadeClient;
		_clock = clock;
		_zoneProvider = zoneProvider;
	}

	public async Task GenerateData()
	{
		var currencies = await _client.GetCurrenciesAsync();
		var euro = currencies.Single(currency => currency.AlphabeticCode is "EUR");
		var dollars = currencies.Single(currency => currency.AlphabeticCode is "USD");

		var employer = await CreateEmployer(euro);
		var bank = await CreateBank(euro);
		var spending = await CreateSpendingAccount(euro, dollars);
		_ = await CreateCashAccount(euro);
		var rando = await CreateRando(euro);
		var otherRando = await CreateAnotherRando(euro);

		await GenerateSalaryTransactions(employer, spending, euro);
		await GenerateRentAndMortgageTransactions(spending, euro, otherRando, bank, rando);

		var zone = _zoneProvider.GetSystemDefault();
		var endTime = _clock.GetCurrentInstant().InZone(zone);
		var startTime = zone.AtStartOfDay(new(endTime.Year - 5, 1, 1));
		var groceryStore = await CreateGroceryStore(euro);
		var groceryProduct = await CreateGroceriesProduct();

		foreach (var date in GetDayInEachWeek(startTime, endTime, zone))
		{
			var factor = Math.Abs(startTime.Year - date.Year);
			var rawAmount = (_random.Next(2, 10) * 10) + (_random.NextDouble() * 5);
			var scaled = rawAmount * Math.Pow(1.2, factor);
			var amount = (decimal)Math.Round(scaled, 2);

			var transactionId = await _client.CreateTransactionAsync(new() { Description = "Groceries" });

			await _client.PutTransferAsync(Guid.NewGuid(), new()
			{
				TransactionId = transactionId,
				SourceAmount = amount,
				SourceAccountId = spending.Currencies.Single(currency => currency.CurrencyId == euro.Id).Id,
				TargetAmount = amount,
				TargetAccountId = groceryStore.Currencies.Single().Id,
				BankReference = null,
				ExternalReference = null,
				InternalReference = null,
				Order = 1,
				BookedAt = date.ToInstant(),
				ValuedAt = null,
			});

			await _client.PutPurchaseAsync(Guid.NewGuid(), new()
			{
				TransactionId = transactionId,
				Price = amount,
				CurrencyId = euro.Id,
				ProductId = groceryProduct.Id,
				Amount = 1,
				DeliveryDate = null,
				Order = 1,
			});

			var transaction = await _client.GetTransactionAsync(transactionId);
			await _client.PutTransactionAsync(transactionId, new()
			{
				Description = transaction.Description,
				ReconciledAt = _clock.GetCurrentInstant(),
			});
		}
	}

	private static IEnumerable<ZonedDateTime> GetDayInEachMonth(ZonedDateTime start, ZonedDateTime end, DateTimeZone zone, int dayOfMonth)
	{
		for (var year = start.Year; year <= end.Year; year++)
		{
			for (var month = year == start.Year ? start.Month : 1; month <= (year == end.Year ? end.Month : 12); month++)
			{
				yield return new(Instant.FromUtc(year, month, dayOfMonth, 10, 00), zone);
			}
		}
	}

	private IEnumerable<ZonedDateTime> GetDayInEachWeek(
		ZonedDateTime start,
		ZonedDateTime end,
		DateTimeZone zone)
	{
		var actualStart = start.DayOfWeek is IsoDayOfWeek.Monday
			? start.LocalDateTime
			: start.LocalDateTime.Next(IsoDayOfWeek.Monday);

		var actualEnd = end.DayOfWeek is IsoDayOfWeek.Sunday
			? end.LocalDateTime
			: end.LocalDateTime.Previous(IsoDayOfWeek.Sunday);

		for (var week = 0; week < Period.DaysBetween(actualStart.Date, actualEnd.Date) / 7; week++)
		{
			yield return (actualStart + Period.FromWeeks(week) + Period.FromDays(_random.Next(7)))
				.InZoneStrictly(zone);
		}
	}

	private async Task GenerateRentAndMortgageTransactions(
		Account spending,
		Currency euro,
		Account otherRando,
		Account bank,
		Account rando)
	{
		var zone = _zoneProvider.GetSystemDefault();
		var endTime = _clock.GetCurrentInstant().InZone(zone);
		var rentStart = zone.AtStartOfDay(new(endTime.Year - 5, 1, 1));
		var startTime = zone.AtStartOfDay(new(endTime.Year - 2, 1, 1));
		var rent = await CreateRentProduct();

		foreach (var date in GetDayInEachMonth(rentStart, startTime, zone, 10))
		{
			var transactionId = await _client.CreateTransactionAsync(new() { Description = "Rent payment" });

			await _client.PutTransferAsync(Guid.NewGuid(), new()
			{
				TransactionId = transactionId,
				SourceAmount = 300,
				SourceAccountId = spending.Currencies.Single(currency => currency.CurrencyId == euro.Id).Id,
				TargetAmount = 300,
				TargetAccountId = otherRando.Currencies.Single().Id,
				BankReference = null,
				ExternalReference = null,
				InternalReference = null,
				Order = 1,
				BookedAt = date.ToInstant(),
				ValuedAt = null,
			});

			await _client.PutPurchaseAsync(Guid.NewGuid(), new()
			{
				TransactionId = transactionId,
				Price = 300,
				CurrencyId = euro.Id,
				ProductId = rent.Id,
				Amount = 1,
				DeliveryDate = null,
				Order = 1,
			});

			var transaction = await _client.GetTransactionAsync(transactionId);
			await _client.PutTransactionAsync(transactionId, new()
			{
				Description = transaction.Description,
				ReconciledAt = _clock.GetCurrentInstant(),
			});
		}

		var loan = await CreateLoan(bank.CounterpartyId, euro);
		var loanProduct = await CreateLoanProduct();
		var houseProduct = await CreateHouseProduct();

		var principalTransactionId = await _client.CreateTransactionAsync(new() { Description = "Principal" });
		await _client.PutTransferAsync(Guid.NewGuid(), new()
		{
			TransactionId = principalTransactionId,
			SourceAmount = loan.Principal,
			SourceAccountId = bank.Currencies.Single().Id,
			TargetAmount = loan.Principal,
			TargetAccountId = spending.Currencies.Single(currency => currency.CurrencyId == euro.Id).Id,
			BankReference = null,
			ExternalReference = null,
			InternalReference = null,
			Order = 1,
			BookedAt = startTime.ToInstant(),
			ValuedAt = null,
		});

		await _client.PutTransferAsync(Guid.NewGuid(), new()
		{
			TransactionId = principalTransactionId,
			SourceAmount = 60_000,
			SourceAccountId = spending.Currencies.Single(currency => currency.CurrencyId == euro.Id).Id,
			TargetAmount = 60_000,
			TargetAccountId = rando.Currencies.Single().Id,
			BankReference = null,
			ExternalReference = null,
			InternalReference = null,
			Order = 2,
			BookedAt = startTime.ToInstant(),
			ValuedAt = null,
		});

		await _client.PutPurchaseAsync(Guid.NewGuid(), new()
		{
			TransactionId = principalTransactionId,
			Price = loan.Principal + 10_000,
			CurrencyId = euro.Id,
			ProductId = houseProduct.Id,
			Amount = 1,
			Order = 1,
		});

		await _client.CreateLoanPaymentAsync(new()
		{
			LoanId = loan.Id,
			TransactionId = principalTransactionId,
			Amount = loan.Principal,
			Interest = 0,
		});

		await _client.CreateLoanPaymentAsync(new()
		{
			LoanId = loan.Id,
			TransactionId = principalTransactionId,
			Amount = -10_000,
			Interest = 0,
		});

		foreach (var date in GetDayInEachMonth(startTime, endTime, zone, 10))
		{
			var transactionId = await _client.CreateTransactionAsync(new() { Description = "Loan payment" });

			await _client.PutTransferAsync(Guid.NewGuid(), new()
			{
				TransactionId = transactionId,
				SourceAmount = 500,
				SourceAccountId = spending.Currencies.Single(currency => currency.CurrencyId == euro.Id).Id,
				TargetAmount = 500,
				TargetAccountId = bank.Currencies.Single().Id,
				BankReference = null,
				ExternalReference = null,
				InternalReference = null,
				Order = 1,
				BookedAt = date.ToInstant(),
				ValuedAt = null,
			});

			await _client.PutTransferAsync(Guid.NewGuid(), new()
			{
				TransactionId = transactionId,
				SourceAmount = 150,
				SourceAccountId = spending.Currencies.Single(currency => currency.CurrencyId == euro.Id).Id,
				TargetAmount = 150,
				TargetAccountId = bank.Currencies.Single().Id,
				BankReference = null,
				ExternalReference = null,
				InternalReference = null,
				Order = 2,
				BookedAt = date.ToInstant(),
				ValuedAt = null,
			});

			await _client.PutPurchaseAsync(Guid.NewGuid(), new()
			{
				TransactionId = transactionId,
				Price = 650,
				CurrencyId = euro.Id,
				ProductId = loanProduct.Id,
				Amount = 1,
				DeliveryDate = null,
				Order = 1,
			});

			await _client.CreateLoanPaymentAsync(new()
			{
				LoanId = loan.Id,
				TransactionId = transactionId,
				Amount = -500,
				Interest = -150,
			});

			var transaction = await _client.GetTransactionAsync(transactionId);
			await _client.PutTransactionAsync(transactionId, new()
			{
				Description = transaction.Description,
				ReconciledAt = _clock.GetCurrentInstant(),
			});
		}
	}

	private async Task GenerateSalaryTransactions(Account employer, Account spending, Currency euro)
	{
		var zone = _zoneProvider.GetSystemDefault();
		var endTime = _clock.GetCurrentInstant().InZone(zone);
		var startTime = zone.AtStartOfDay(new(endTime.Year - 5, 1, 1));
		var salary = await CreateSalary();

		foreach (var date in GetDayInEachMonth(startTime, endTime, zone, 5))
		{
			var factor = Math.Abs(startTime.Year - date.Year);
			var amount = (decimal)Math.Round(1000 * Math.Pow(1.1, factor), 2);
			var transactionId = await _client.CreateTransactionAsync(new() { Description = $"Salary for {date.Date}" });
			await _client.PutTransferAsync(Guid.NewGuid(), new()
			{
				TransactionId = transactionId,
				SourceAmount = amount,
				SourceAccountId = employer.Currencies.Single().Id,
				TargetAmount = amount,
				TargetAccountId = spending.Currencies.Single(currency => currency.CurrencyId == spending.PreferredCurrencyId).Id,
				ExternalReference = null,
				InternalReference = null,
				Order = 1,
				BookedAt = date.ToInstant(),
				ValuedAt = null,
			});

			await _client.PutPurchaseAsync(Guid.NewGuid(), new()
			{
				TransactionId = transactionId,
				Price = amount,
				CurrencyId = euro.Id,
				ProductId = salary.Id,
				Amount = 1,
				Order = 1,
			});

			var transaction = await _client.GetTransactionAsync(transactionId);
			await _client.PutTransactionAsync(transactionId, new()
			{
				OwnerId = transaction.OwnerId,
				Description = transaction.Description,
				ReconciledAt = _clock.GetCurrentInstant(),
			});
		}
	}

	private async Task<Account> CreateEmployer(Currency currency) => await CreateCounterpartyWithAccount(
		"Acme Corporation",
		creation => creation.Iban = "BH75IYNF73521P946957XJ",
		currency);

	private async Task<Account> CreateBank(Currency currency) => await CreateCounterpartyWithAccount(
		"Woodgrove Bank",
		creation =>
		{
			creation.Iban = "BG94JZKB6860079N098108";
			creation.Bic = "ESOEZMV1";
		},
		currency);

	private async Task<Account> CreateRando(Currency currency) => await CreateCounterpartyWithAccount(
		"Rob Rando",
		creation => creation.Iban = "IT75J005100488539935351257X",
		currency);

	private async Task<Account> CreateAnotherRando(Currency currency) => await CreateCounterpartyWithAccount(
		"John Rando",
		creation => creation.Iban = "RO32VUABU34L17920P26R50Y",
		currency);

	private async Task<Account> CreateGroceryStore(Currency currency) => await CreateCounterpartyWithAccount(
		"Kwik-E-Mart",
		creation => creation.Iban = "SM14X0099836023241577WI489R",
		currency);

	private async Task<Account> CreateSpendingAccount(Currency euro, Currency dollars) => await CreateAccount(
		"Spending",
		[euro, dollars],
		null,
		creation => creation.Iban = "MU20WEYQ0941005006007662698AER");

	private async Task<Account> CreateCashAccount(Currency euro) => await CreateAccount(
		"Cash",
		euro);

	private async Task<Product> CreateSalary()
	{
		if ((await _client.GetUnitsAsync()).SingleOrDefault(unit => unit.Name is "Month") is not { } monthUnit)
		{
			var unitId = Guid.NewGuid();
			await _client.PutUnitAsync(unitId, new() { Name = "Month" });
			monthUnit = await _client.GetUnitAsync(unitId);
		}

		if ((await _client.GetProductsAsync()).SingleOrDefault(product => product.Name is "Salary") is { } salaryProduct)
		{
			return salaryProduct;
		}

		var productId = Guid.NewGuid();
		await _client.PutProductAsync(productId, new()
		{
			Name = "Salary",
			UnitId = monthUnit.Id,
		});

		return await _client.GetProductAsync(productId);
	}

	private async Task<Loan> CreateLoan(Guid bankId, Currency euro)
	{
		const string name = "Woodgrove Bank Mortgage";

		if ((await _client.GetLoansAsync()).SingleOrDefault(loan => loan.Name is name) is { } mortgage)
		{
			return mortgage;
		}

		var counterparty = await _client.GetMyCounterpartyAsync();
		var loanId = await _client.CreateLoanAsync(new()
		{
			Name = name,
			IssuingCounterpartyId = bankId,
			ReceivingCounterpartyId = counterparty.Id,
			Principal = 50_000,
			CurrencyId = euro.Id,
		});

		return await _client.GetLoanAsync(loanId);
	}

	private async Task<Product> CreateLoanProduct()
	{
		const string categoryName = "Liabilities";
		const string name = "Loan";

		if ((await _client.GetCategoriesAsync()).SingleOrDefault(category => category.Name is categoryName) is not { } liabilities)
		{
			var categoryId = await _client.CreateCategoryAsync(new() { Name = categoryName });
			liabilities = await _client.GetCategoryAsync(categoryId);
		}

		if ((await _client.GetProductsAsync()).SingleOrDefault(product => product.Name is name) is { } loan)
		{
			return loan;
		}

		var productId = Guid.NewGuid();
		await _client.PutProductAsync(productId, new()
		{
			Name = name,
			CategoryId = liabilities.Id,
		});

		return await _client.GetProductAsync(productId);
	}

	private async Task<Product> CreateGroceriesProduct()
	{
		const string categoryName = "Groceries";
		const string name = "Groceries";

		if ((await _client.GetCategoriesAsync()).SingleOrDefault(category => category.Name is categoryName) is not { } groceries)
		{
			var categoryId = await _client.CreateCategoryAsync(new() { Name = categoryName });
			groceries = await _client.GetCategoryAsync(categoryId);
		}

		if ((await _client.GetProductsAsync()).SingleOrDefault(product => product.Name is name) is { } loan)
		{
			return loan;
		}

		var productId = Guid.NewGuid();
		await _client.PutProductAsync(productId, new()
		{
			Name = name,
			CategoryId = groceries.Id,
		});

		return await _client.GetProductAsync(productId);
	}

	private async Task<Product> CreateHouseProduct()
	{
		const string name = "House";

		if ((await _client.GetProductsAsync()).SingleOrDefault(product => product.Name is name) is { } house)
		{
			return house;
		}

		var productId = Guid.NewGuid();
		await _client.PutProductAsync(productId, new() { Name = name });
		return await _client.GetProductAsync(productId);
	}

	private async Task<Product> CreateRentProduct()
	{
		if ((await _client.GetUnitsAsync()).SingleOrDefault(unit => unit.Name is "Month") is not { } monthUnit)
		{
			var unitId = Guid.NewGuid();
			await _client.PutUnitAsync(unitId, new() { Name = "Month" });
			monthUnit = await _client.GetUnitAsync(unitId);
		}

		const string name = "Rent";

		if ((await _client.GetCategoriesAsync()).SingleOrDefault(category => category.Name is name) is not { } rentCategory)
		{
			var categoryId = Guid.NewGuid();
			await _client.PutCategoryAsync(categoryId, new() { Name = name, LinkProduct = true });
			rentCategory = await _client.GetCategoryAsync(categoryId);
		}

		if ((await _client.GetProductsAsync()).SingleOrDefault(product => product.Name is name) is { } rent)
		{
			return rent;
		}

		var productId = Guid.NewGuid();
		await _client.PutProductAsync(productId, new()
		{
			Name = name,
			UnitId = monthUnit.Id,
			CategoryId = rentCategory.Id,
		});

		return await _client.GetProductAsync(productId);
	}

	private async Task<Account> CreateCounterpartyWithAccount(string name, Action<AccountCreation> config, params Currency[] currencies)
	{
		var counterparties = await _client.GetCounterpartiesAsync();

		var counterpartyId = counterparties.SingleOrDefault(counterparty => counterparty.Name == name) is { } existingCounterparty
			? existingCounterparty.Id
			: await _client.CreateCounterpartyAsync(new() { Name = name });

		var counterparty = await _client.GetCounterpartyAsync(counterpartyId);
		return await CreateAccount(name, currencies, counterparty, config);
	}

	private async Task<Account> CreateAccount(string name, Currency currency, Counterparty? counterparty = null)
	{
		return await CreateAccount(name, [currency], counterparty);
	}

	private async Task<Account> CreateAccount(string name, Currency[] currencies, Counterparty? counterparty = null, Action<AccountCreation>? config = null)
	{
		var accounts = await _client.GetAccountsAsync();
		if (accounts.SingleOrDefault(account => account.Name == name) is { } existingAccount)
		{
			return existingAccount;
		}

		counterparty ??= await _client.GetMyCounterpartyAsync();

		var accountCreation = new AccountCreation
		{
			Name = name,
			CounterpartyId = counterparty.Id,
			PreferredCurrencyId = currencies.First().Id,
			AccountNumber = null,
			Currencies = currencies
				.Select(currency => new AccountInCurrencyCreation { CurrencyId = currency.Id })
				.ToList(),
		};

		config?.Invoke(accountCreation);

		var spendingAccountId = await _client.CreateAccountAsync(accountCreation);
		return await _client.GetAccountAsync(spendingAccountId);
	}
}
