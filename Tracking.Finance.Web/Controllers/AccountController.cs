using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Tracking.Finance.Web.Data;
using Tracking.Finance.Web.Data.Models;
using Tracking.Finance.Web.Models;
using Tracking.Finance.Web.Models.Accounts;

using VMelnalksnis.SvgCharts.Charts;

namespace Tracking.Finance.Web.Controllers
{
	/// <summary>
	/// MVC controller for managing <see cref="Account"/> and <see cref="AccountInCurrency"/> entities.
	/// </summary>
	[Authorize]
	public class AccountController : FinanceController
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AccountController"/> class.
		/// </summary>
		/// <param name="dbContext"><see cref="DbContext"/> for accessing and modifying finance data.</param>
		/// <param name="userManager"><see cref="UserManager{TUser}"/> for getting the currently authenticated user.</param>
		public AccountController(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
			: base(dbContext, userManager)
		{
		}

		/// <summary>
		/// Get an overview of all accounts.
		/// </summary>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
		///
		/// <returns>
		/// A <see cref="ViewResult"/> containing information about all accounts.
		/// </returns>
		[HttpGet]
		public async Task<ViewResult> Index(CancellationToken cancellationToken)
		{
			var financeUser = await GetCurrentUser(cancellationToken);

			var accounts =
				await DbContext.Accounts
					.WhichBelongToUser(financeUser)
					.Include(account => account.Counterparty)
					.Include(account => account.AccountsInCurrencies)
					.ThenInclude(account => account.Currency)
					.ToListAsync(cancellationToken);

			var viewModel =
				accounts
					.Select(account =>
						new AccountIndexViewModel(
							account.Id,
							account.Name,
							financeUser.CounterpartyId.HasValue && financeUser.CounterpartyId == account.CounterpartyId))
					.ToList();

			return View(viewModel);
		}

		/// <summary>
		/// Get details about the specified <see cref="Account"/>.
		/// </summary>
		/// <param name="id">The id of the <see cref="Account"/>.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
		///
		/// <returns>
		/// A <see cref="ViewResult"/> containing information about the specified <see cref="Account"/>.
		/// </returns>
		[HttpGet]
		public async Task<ViewResult> Details(int id, CancellationToken cancellationToken)
		{
			var financeUser = await GetCurrentUser(cancellationToken);

			var account =
				await DbContext.Accounts
					.WhichBelongToUser(financeUser)
					.WithId(id)
					.Include(account => account.AccountsInCurrencies)
					.ThenInclude(accountInCurrency => accountInCurrency.Currency)
					.SingleAsync(cancellationToken);

			var accountsInCurrencies = account.AccountsInCurrencies;

			var transactionsFromAccount =
				(await DbContext.Transactions
					.WhichBelongToUser(financeUser)
					.Include(transaction => transaction.TransactionItems)
					.ThenInclude(item => item.SourceAccount)
					.ToListAsync(cancellationToken))
					.SelectMany(transaction => transaction.TransactionItems)
					.Where(item => accountsInCurrencies.Select(aic => aic.Id).Contains(item.SourceAccountId));

			var transactionsToAccount =
				(await DbContext.Transactions
					.WhichBelongToUser(financeUser)
					.Include(transaction => transaction.TransactionItems)
					.ThenInclude(item => item.TargetAccount)
					.ToListAsync(cancellationToken))
					.SelectMany(transaction => transaction.TransactionItems)
					.Where(item => accountsInCurrencies.Select(aic => aic.Id).Contains(item.TargetAccountId));

			var viewModels =
				accountsInCurrencies
					.Select(currencyAccount =>
					{
						var fromAccount =
							transactionsFromAccount
								.Where(item => item.SourceAccountId == currencyAccount.Id)
								.Sum(item => item.SourceAmount);

						var toAccount =
							transactionsToAccount
								.Where(item => item.TargetAccountId == currencyAccount.Id)
								.Sum(item => item.TargetAmount);

						return
							new AccountDetailsCurrencyViewModel(
								currencyAccount.Currency.AlphabeticCode,
								toAccount,
								fromAccount);
					})
					.ToList();

			var existingCurrencies = accountsInCurrencies.Select(ac => ac.CurrencyId).ToList();

			var currencies =
				await DbContext.Currencies
					.Where(currency => !existingCurrencies.Contains(currency.Id))
					.ToListAsync(cancellationToken);

			var lineChart = new LineChart
			{
				ViewBox = new Rectangle(0, 0, 500, 100),
				Datasets =
				new List<Dataset>
				{
					new Dataset(new List<Point> { new Point(0, 40), new Point(100, 100), new Point(200, 0), }),
				},
			};

			var viewModel =
				new AccountDetailsViewModel(
					account.Id,
					account.Name,
					lineChart,
					currencies.GetSelectListItems(),
					viewModels);

			return View(viewModel);
		}

		/// <summary>
		/// Add another supported currency to an <see cref="Account"/>.
		/// </summary>
		/// <param name="model">The details needed for adding a currency to an account.</param>
		///
		/// <returns>
		/// <see cref="RedirectToActionResult"/> if currency is successfully added.
		/// </returns>
		[HttpPost]
		public async Task<IActionResult> AddCurrency(AddCurrencyModel model)
		{
			var financeUser = await GetCurrentUser();

			if (!ModelState.IsValid)
			{
				throw new NotImplementedException();
			}

			// todo validation
			var account =
				await DbContext.Accounts
					.WhichBelongToUser(financeUser)
					.WithId(model.AccountId!.Value)
					.SingleAsync();

			var currency =
				await DbContext.Currencies
					.WithId(model.CurrencyId!.Value)
					.SingleAsync();

			var accountInCurrency = new AccountInCurrency
			{
				FinanceUserId = financeUser.Id,
				AccountId = account.Id,
				CurrencyId = currency.Id,
			}.CreatedAndModifiedNow();

			var entity = await DbContext.AccountsInCurrencies.AddAsync(accountInCurrency);
			await SaveChangesAsync();

			return RedirectToAction(nameof(Details), new { id = account.Id });
		}

		/// <summary>
		/// Create a new <see cref="Account"/>.
		/// </summary>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
		///
		/// <returns>
		/// A <see cref="ViewResult"/> for creating a new <see cref="Account"/>.
		/// </returns>
		[HttpGet]
		public async Task<ViewResult> Create(CancellationToken cancellationToken)
		{
			var financeUser = await GetCurrentUser(cancellationToken);
			var currencies = await DbContext.Currencies.ToListAsync(cancellationToken);
			var currencyItems = currencies.GetSelectListItems();

			var viewModel = new AccountCreationModel
			{
				FinanceUserId = financeUser.Id,
				Currencies = currencyItems,
			};

			return View(viewModel);
		}

		/// <summary>
		/// Create a new <see cref="Account"/>.
		/// </summary>
		/// <param name="model">The details needed for creating an account.</param>
		///
		/// <returns>
		/// <see cref="RedirectToActionResult"/> if <paramref name="model"/> is valid; otherwise <see cref="ViewResult"/>.
		/// </returns>
		[HttpPost]
		public async Task<IActionResult> Create(AccountCreationModel model)
		{
			// todo move validation
			if (model.SingleCurrency && model.CurrencyId is null)
			{
				ModelState.AddModelError(nameof(AccountCreationModel.CurrencyId), "Currency is required if account is in single currency");
			}

			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var account = new Account
			{
				FinanceUserId = model.FinanceUserId!.Value,
			}.WithName(model.Name).CreatedAndModifiedNow();

			var accountEntity = await DbContext.AddAsync(account);
			await SaveChangesAsync();

			if (model.CurrencyId.HasValue)
			{
				var accountInCurrency = new AccountInCurrency
				{
					FinanceUserId = model.FinanceUserId.Value,
					AccountId = accountEntity.Entity.Id,
					CurrencyId = model.CurrencyId.Value,
				}.CreatedAndModifiedNow();

				var accountInCurrencyEntity = await DbContext.AddAsync(accountInCurrency);
				await SaveChangesAsync();
			}

			return RedirectToAction(nameof(Details), new { id = accountEntity.Entity.Id });
		}
	}
}
