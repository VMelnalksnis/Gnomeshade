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
using Tracking.Finance.Web.Models.Units;

namespace Tracking.Finance.Web.Controllers
{
	[Authorize]
	public class UnitController : FinanceController
	{
		public UnitController(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
			: base(dbContext, userManager)
		{
		}

		[HttpGet]
		public async Task<ViewResult> Index(CancellationToken cancellationToken)
		{
			var financeUser = await GetCurrentUser(cancellationToken);
			var units =
				await DbContext.Units
					.WhichBelongToUser(financeUser)
					.ToListAsync(cancellationToken);

			var viewModel =
				units
					.Select(unit => new UnitIndexModel(unit.Id, unit.Name, unit.Exponent, unit.Mantissa))
					.ToList();

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
			var units =
				await DbContext.Units
					.WhichBelongToUser(financeUser)
					.ToListAsync(cancellationToken);

			var model = new UnitCreationModel
			{
				FinanceUserId = financeUser.Id,
				Units = units.GetSelectListItemsWithDefault(),
			};

			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> Create(UnitCreationModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var unit = new Unit
			{
				FinanceUserId = model.FinanceUserId.Value,
				Exponent = model.Exponent.Value,
				Mantissa = model.Mantissa.Value,
			}.WithName(model.Name).CreatedAndModifiedNow();

			var unitEntity = await DbContext.Units.AddAsync(unit);
			await SaveChangesAsync();

			return RedirectToAction(nameof(Details), new { id = unitEntity.Entity.Id });
		}
	}
}
