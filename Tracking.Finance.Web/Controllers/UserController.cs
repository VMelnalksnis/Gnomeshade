using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Tracking.Finance.Web.Data;
using Tracking.Finance.Web.Models;
using Tracking.Finance.Web.Models.Users;

namespace Tracking.Finance.Web.Controllers
{
	[Authorize]
	public class UserController : FinanceController
	{
		public UserController(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
			: base(dbContext, userManager)
		{
		}

		[HttpGet]
		public async Task<ViewResult> Index(CancellationToken cancellationToken)
		{
			var financeUser = await GetCurrentUser(cancellationToken);
			var counterparties =
				await DbContext.Counterparties
					.WhichBelongToUser(financeUser)
					.ToListAsync(cancellationToken);

			var viewModel =
				new UserIndexModel
				{
					IdentityUserId = financeUser.IdentityUser.Id,
					IdentityUserName = financeUser.IdentityUser.UserName,
					CounterpartyId = financeUser.Counterparty?.Id,
					CounterpartyName = financeUser.Counterparty?.Name,
					Counterparties = counterparties.GetSelectListItemsWithDefault(),
				};

			return View(viewModel);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(UserUpdateModel model)
		{
			var financeUser = await GetCurrentUser();

			financeUser.CounterpartyId = model.CounterpartyId;
			await SaveChangesAsync();

			return RedirectToAction(nameof(Index));
		}
	}
}
