﻿using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

using Tracking.Finance.Web.Data.Models;

namespace Tracking.Finance.Web.Data
{
	public static class ApplicationBuilderExtensions
	{
		public static async Task SeedDatabase(this IServiceProvider serviceProvider)
		{
			// By default IdentityUser expects UserName to be an email address
			// If that is not respected, either views or managers/stores need to be modified
			var testUser =
				new IdentityUser
				{
					Email = "foo@bar.com",
					NormalizedEmail = "FOO@BAR.COM",
					EmailConfirmed = true,
					UserName = "foo@bar.com",
					NormalizedUserName = "FOO@BAR.COM",
					PhoneNumber = "+1-202-555-0176",
					PhoneNumberConfirmed = true,
					SecurityStamp = Guid.NewGuid().ToString("D"),
				};

			var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
			var result = await userManager.CreateAsync(testUser, "Password1!");
			if (!result.Succeeded)
			{
				throw new ApplicationException("Failed to seed database");
			}

			var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

			var financeUser = (await context.FinanceUsers.AddAsync(
				new FinanceUser
				{
					IdentityUserId = context.Users.Single().Id,
				})).Entity;
			await context.SaveChangesAsync();

			await context.Currencies.AddRangeAsync(
				new Currency
				{
					AlphabeticCode = "EUR",
					Crypto = false,
					NumericCode = 978,
					Name = "Euro",
					From = new DateTimeOffset(new DateTime(1999, 1, 1), TimeSpan.Zero),
					Historical = false,
					MinorUnit = 2,
					Official = true,
					Until = null,
				},
				new Currency
				{
					AlphabeticCode = "USD",
					Crypto = false,
					NumericCode = 840,
					Name = "United Stated dollar",
					From = new DateTimeOffset(new DateTime(1792, 4, 2), TimeSpan.Zero),
					Historical = false,
					MinorUnit = 2,
					Official = true,
					Until = null,
				});
			await context.SaveChangesAsync();

			await context.Accounts.AddRangeAsync(
				new Account
				{
					FinanceUserId = financeUser.Id,
					Name = "Spending",
					NormalizedName = "SPENDING",
					SingleCurrency = true,
				},
				new Account
				{
					FinanceUserId = financeUser.Id,
					Name = "Cash",
					NormalizedName = "CASH",
					SingleCurrency = false,
				});
			await context.SaveChangesAsync();

			var accountInCurrency = await context.AccountsInCurrencies.AddAsync(
				new AccountInCurrency
				{
					AccountId = context.Accounts.Single(account => account.Name == "Spending").Id,
					CurrencyId = context.Currencies.Single(currency => currency.AlphabeticCode == "EUR").Id,
					FinanceUserId = financeUser.Id,
				});
			await context.SaveChangesAsync();

			var transactionCategory = await context.TransactionCategories.AddAsync(
				new TransactionCategory
				{
					FinanceUserId = financeUser.Id,
					Name = "Groceries",
					NormalizedName = "GROCERIES",
				});

			await context.SaveChangesAsync();

			await context.Counterparties.AddAsync(
				new Counterparty
				{
					FinanceUserId = financeUser.Id,
					Name = financeUser.IdentityUser.UserName,
					NormalizedName = financeUser.IdentityUser.NormalizedUserName,
				});

			await context.Products.AddAsync(
				new Product
				{
					FinanceUserId = financeUser.Id,
					Name = "Test product",
					NormalizedName = "TEST PRODUCT",
				});

			await context.SaveChangesAsync();
		}
	}
}
