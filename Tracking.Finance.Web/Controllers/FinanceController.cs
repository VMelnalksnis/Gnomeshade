using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Tracking.Finance.Web.Data;
using Tracking.Finance.Web.Data.Models;

namespace Tracking.Finance.Web.Controllers
{
	public abstract class FinanceController : Controller
	{
		public FinanceController(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
		{
			DbContext = dbContext;
			UserManager = userManager;
		}

		protected ApplicationDbContext DbContext { get; }

		protected UserManager<IdentityUser> UserManager { get; }

		protected async Task<FinanceUser> GetCurrentUser(CancellationToken cancellationToken = default)
		{
			var identityUser = await UserManager.GetUserAsync(User);
			var financeUser =
				await DbContext.FinanceUsers
					.SingleAsync(financeUser => financeUser.IdentityUserId == identityUser.Id, cancellationToken);

			return financeUser;
		}
	}
}
