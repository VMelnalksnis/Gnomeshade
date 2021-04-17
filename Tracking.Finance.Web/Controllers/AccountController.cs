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

		public async Task<ViewResult> Index(CancellationToken cancellationToken)
		{
			var accounts = await GetCurrentUserAccounts(cancellationToken);
			var viewModel = new AccountIndexViewModel(accounts);
			return View(viewModel);
		}

		public async Task<ViewResult> Details(int id, CancellationToken cancellationToken)
		{
			var accounts = await GetCurrentUserAccounts(cancellationToken);
			var selectedAccount = accounts.Single(account => account.Id == id);
			var viewModel = new AccountDetailsViewModel(selectedAccount);
			return View(viewModel);
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
