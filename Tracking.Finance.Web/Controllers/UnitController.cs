using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Tracking.Finance.Web.Data;

namespace Tracking.Finance.Web.Controllers
{
	public class UnitController : FinanceController
	{
		public UnitController(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
			: base(dbContext, userManager)
		{
		}

		[HttpGet]
		public async Task<ViewResult> Index(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		[HttpGet]
		public async Task<ViewResult> Details(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		[HttpGet]
		public async Task<ViewResult> Create(CancellationToken cancellation)
		{
			throw new NotImplementedException();
		}

		[HttpPost]
		public async Task<IActionResult> Create(UnitCreationModel model)
		{
			throw new NotImplementedException();
		}
	}
}
