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
	/// <summary>
	/// MVC controller for managing <see cref="Unit"/> and <see cref="UnitClosure"/> entities.
	/// </summary>
	[Authorize]
	public class UnitController : FinanceController
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="UnitController"/> class.
		/// </summary>
		/// <param name="dbContext"><see cref="DbContext"/> for accessing and modifying finance data.</param>
		/// <param name="userManager"><see cref="UserManager{TUser}"/> for getting the currently authenticated user.</param>
		public UnitController(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
			: base(dbContext, userManager)
		{
		}

		/// <summary>
		/// Get an overview of all units.
		/// </summary>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
		///
		/// <returns>
		/// A <see cref="ViewResult"/> containing information about all units.
		/// </returns>
		[HttpGet]
		public async Task<ViewResult> Index(CancellationToken cancellationToken)
		{
			var financeUser = await GetCurrentUser(cancellationToken);
			var units =
				await DbContext.Units
					.WhichBelongToUser(financeUser)
					.ToListAsync(cancellationToken);

			var closures =
				units
					.Select(unit => new { unit, parentClosure = DbContext.UnitClosures.Where(closure => closure.ChildUnitId == unit.Id).SingleOrDefault() })
					.Select(unit => new { unit.unit, parentUnit = units.SingleOrDefault(u => u.Id == unit.parentClosure?.ParentUnitId) })
					.ToList();

			var viewModel =
				closures
					.Select(unit => new UnitIndexModel(unit.unit.Id, unit.unit.Name, unit.unit.Exponent, unit.unit.Mantissa, unit.parentUnit?.Id, unit.parentUnit?.Name))
					.ToList();

			return View(viewModel);
		}

		/// <summary>
		/// Get details about the specified <see cref="Unit"/>.
		/// </summary>
		/// <param name="id">The id of the <see cref="Unit"/>.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
		///
		/// <returns>
		/// A <see cref="ViewResult"/> containing information about the specified <see cref="Unit"/>.
		/// </returns>
		[HttpGet]
		public Task<ViewResult> Details(int id, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Create a new <see cref="Unit"/>.
		/// </summary>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
		///
		/// <returns>
		/// A <see cref="ViewResult"/> for creating a new <see cref="Unit"/>.
		/// </returns>
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

		/// <summary>
		/// Create a new <see cref="Unit"/>.
		/// </summary>
		/// <param name="model">The details needed for creating a unit.</param>
		///
		/// <returns>
		/// <see cref="RedirectToActionResult"/> if <paramref name="model"/> is valid; otherwise <see cref="ViewResult"/>.
		/// </returns>
		[HttpPost]
		public async Task<IActionResult> Create(UnitCreationModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var unit = new Unit
			{
				FinanceUserId = model.FinanceUserId!.Value,
				Exponent = model.Exponent,
				Mantissa = model.Mantissa,
			}.WithName(model.Name).CreatedAndModifiedNow();

			var unitEntity = await DbContext.Units.AddAsync(unit);
			await SaveChangesAsync();

			if (model.ParentUnitId.HasValue)
			{
				var financeUser = await GetCurrentUser();
				var parentUnit =
					await DbContext.Units
						.WhichBelongToUser(financeUser)
						.WithId(model.ParentUnitId.Value)
						.SingleAsync();

				var closure = new UnitClosure
				{
					ChildUnitId = unitEntity.Entity.Id,
					ParentUnitId = parentUnit.Id,
				};

				var closureEntity = await DbContext.UnitClosures.AddAsync(closure);
				await SaveChangesAsync();
			}

			return RedirectToAction(nameof(Index));

			// return RedirectToAction(nameof(Details), new { id = unitEntity.Entity.Id });
		}
	}
}
