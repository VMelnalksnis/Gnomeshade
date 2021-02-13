using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Tracking.Finance.Web.Data;
using Tracking.Finance.Web.Data.Models;
using Tracking.Finance.Web.Models;

namespace Tracking.Finance.Web.Controllers
{
	[Authorize]
	public class AccountController : Controller
	{
		private readonly ApplicationDbContext _dbContext;
		private readonly UserManager<IdentityUser> _userManager;

		/// <summary>
		/// Initializes a new instance of the <see cref="AccountController"/> class.
		/// </summary>
		public AccountController(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
		{
			_dbContext = dbContext;
			_userManager = userManager;
		}

		public async Task<ViewResult> Index()
		{
			var accounts = await GetCurrentUserAccounts();
			return View(new AccountsIndexViewModel(accounts));
		}

		public async Task<ViewResult> Details(int id)
		{
			var accounts = await GetCurrentUserAccounts();
			var selectedAccount = accounts.Single(account => account.Id == id);
			return View(new AccountDetailsViewModel(selectedAccount));
		}

		private async Task<List<Account>> GetCurrentUserAccounts()
		{
			var identityUserTask = _userManager.GetUserAsync(User);
			var financeUsers =
				_dbContext
					.FinanceUsers
					.Include(user => user.Accounts)
					.ThenInclude(account => account.AccountsInCurrencies)
					.ThenInclude(accountInCurrency => accountInCurrency.Currency);

			var identityUser = await identityUserTask;
			var financeUser = financeUsers.Single(user => user.IdentityUserId == identityUser.Id);
			return financeUser.Accounts.ToList();
		}
	}
}
