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
			var accounts = await GetCurrentUserAccounts(cancellationToken);
			var viewModel = new AccountIndexViewModel(accounts);
			return View(viewModel);
		}

		[HttpGet]
		public async Task<ViewResult> Details(int id, CancellationToken cancellationToken)
		{
			var accounts = await GetCurrentUserAccounts(cancellationToken);
			var selectedAccount = accounts.Single(account => account.Id == id);
			var viewModel = new AccountDetailsViewModel(selectedAccount);
			return View(viewModel);
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
				Name = model.Name,
				NormalizedName = model.Name.ToUpperInvariant().Trim(),
				SingleCurrency = model.SingleCurrency,
			}.SetCreationDate();

			var accountEntity = await DbContext.AddAsync(account);
			_ = await DbContext.SaveChangesAsync();

			if (model.CurrencyId.HasValue)
			{
				var accountInCurrency = new AccountInCurrency
				{
					FinanceUserId = model.FinanceUserId.Value,
					AccountId = accountEntity.Entity.Id,
					CurrencyId = model.CurrencyId.Value,
				}.SetCreationDate();

				var accountInCurrencyEntity = await DbContext.AddAsync(accountInCurrency);
				_ = await DbContext.SaveChangesAsync();
			}

			return RedirectToAction(nameof(Details), new { id = accountEntity.Entity.Id });
		}

		private async Task<List<Account>> GetCurrentUserAccounts(CancellationToken cancellationToken)
		{
			var financeUser = await GetCurrentUser(cancellationToken);

			var accounts =
				await DbContext.Accounts
					.WhichBelongToUser(financeUser)
					.Include(account => account.AccountsInCurrencies)
					.ThenInclude(account => account.Currency)
					.ToListAsync(cancellationToken);

			return accounts;
		}
	}
}
