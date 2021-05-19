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

namespace Tracking.Finance.Web.Controllers
{
	[Authorize]
	public class AccountController : FinanceController
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AccountController"/> class.
		/// </summary>
		public AccountController(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
			: base(dbContext, userManager)
		{
		}

		[HttpGet]
		public async Task<ViewResult> Index(CancellationToken cancellationToken)
		{
			var financeUser = await GetCurrentUser(cancellationToken);

			var accounts =
				await DbContext.Accounts
					.WhichBelongToUser(financeUser)
					.Include(account => account.AccountsInCurrencies)
					.ThenInclude(account => account.Currency)
					.ToListAsync(cancellationToken);

			var viewModel = new AccountIndexViewModel(accounts);
			return View(viewModel);
		}

		[HttpGet]
		public async Task<ViewResult> Details(int id, CancellationToken cancellationToken)
		{
			var financeUser = await GetCurrentUser(cancellationToken);

			var account =
				await DbContext.Accounts
					.WhichBelongToUser(financeUser)
					.Where(account => account.Id == id)
					.Include(account => account.AccountsInCurrencies)
					.ThenInclude(accountInCurrency => accountInCurrency.Currency)
					.SingleAsync(cancellationToken);

			var accountsInCurrencies = account.AccountsInCurrencies;

			var transactionsFromAccount =
				(await DbContext.Transactions
					.WhichBelongToUser(financeUser)
					.Where(transaction => transaction.SourceAccountId == account.Id)
					.Include(transaction => transaction.TransactionItems)
					.ToListAsync(cancellationToken))
					.SelectMany(transaction => transaction.TransactionItems);

			var transactionsToAccount =
				(await DbContext.Transactions
					.WhichBelongToUser(financeUser)
					.Where(transaction => transaction.TargetAccountId == account.Id)
					.Include(transaction => transaction.TransactionItems)
					.ToListAsync(cancellationToken))
					.SelectMany(transaction => transaction.TransactionItems);

			var viewModels =
				accountsInCurrencies
					.Select(ac =>
					{
						var fromAccount =
							transactionsFromAccount
								.Where(item => item.SourceCurrencyId == ac.CurrencyId)
								.Select(item => item.SourceAmount)
								.Sum();

						var toAccount =
							transactionsToAccount
								.Where(item => item.TargetCurrencyId == ac.CurrencyId)
								.Select(item => item.TargetAmount)
								.Sum();

						return new AccountDetailsCurrencyViewModel(ac, toAccount, fromAccount, toAccount - fromAccount);
					})
					.ToList();

			var existingCurrencies = accountsInCurrencies.Select(ac => ac.CurrencyId).ToList();

			var currencies =
				await DbContext.Currencies
					.Where(currency => !existingCurrencies.Contains(currency.Id))
					.ToListAsync(cancellationToken);

			var currencyItems = currencies.GetSelectListItems();

			var viewModel = new AccountDetailsViewModel(account, viewModels, currencyItems);
			return View(viewModel);
		}

		[HttpPost]
		public async Task<IActionResult> AddCurrency(AddCurrencyModel model)
		{
			var financeUser = await GetCurrentUser();

			// todo validation
			var account =
				await DbContext.Accounts
					.WhichBelongToUser(financeUser)
					.Where(account => account.Id == model.AccountId)
					.SingleAsync();

			var currency =
				await DbContext.Currencies
					.Where(currency => currency.Id == model.CurrencyId)
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
				FinanceUserId = model.FinanceUserId.Value,
				SingleCurrency = model.SingleCurrency,
				UserAccount = model.UserAccount,
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
