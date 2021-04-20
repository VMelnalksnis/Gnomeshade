using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Tracking.Finance.Web.Data;

namespace Tracking.Finance.Web.Controllers
{
	public class CurrencyController : FinanceController
	{
		public CurrencyController(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
			: base(dbContext, userManager)
		{
		}

		[HttpGet]
		public async Task<ViewResult> Index(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		[HttpGet]
		public async Task<ViewResult> Details(int id, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
