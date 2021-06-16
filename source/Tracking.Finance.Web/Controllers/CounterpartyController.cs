using System;
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
using Tracking.Finance.Web.Models.Counterparties;

namespace Tracking.Finance.Web.Controllers
{
	[Authorize]
	public class CounterpartyController : FinanceController
	{
		public CounterpartyController(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
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

			var viewModel = counterparties.GetViewModels().ToList();
			return View(viewModel);
		}

		[HttpGet]
		public async Task<ViewResult> Details(int id, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		[HttpGet]
		public async Task<ViewResult> Create(CancellationToken cancellationToken)
		{
			var financeUser = await GetCurrentUser(cancellationToken);
			var viewModel = new CounterpartyCreationModel { FinanceUserId = financeUser.Id };
			return View(viewModel);
		}

		[HttpPost]
		public async Task<IActionResult> Create(CounterpartyCreationModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var counterparty =
				new Counterparty { FinanceUserId = model.FinanceUserId.Value }
					.WithName(model.Name)
					.CreatedAndModifiedNow();

			var entity = await DbContext.Counterparties.AddAsync(counterparty);
			await SaveChangesAsync();

			return RedirectToAction(nameof(Index)); // todo details
		}
	}
}
