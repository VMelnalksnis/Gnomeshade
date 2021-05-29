using System.Diagnostics;
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
using Tracking.Finance.Web.Models.Filters;

namespace Tracking.Finance.Web.Controllers.Api
{
	[Authorize]
	[Route("api/[controller]")]
	[ApiController]
	public class AccountController : ControllerBase
	{
		private readonly ApplicationDbContext _dbContext;
		private readonly UserManager<IdentityUser> _userManager;

		public AccountController(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
		{
			_dbContext = dbContext;
			_userManager = userManager;
		}

		[ValidateModel]
		[Route("api/account")]
		[HttpPost]
		public async Task<IActionResult> Create([FromBody] AccountCreationModel model)
		{
			var account = new Account
			{
				FinanceUserId = model.FinanceUserId!.Value,
				PrefferedCurrencyId = model.CurrencyId.Value,
			}.WithName(model.Name).CreatedAndModifiedNow();

			var accountEntity = await _dbContext.AddAsync(account);
			await SaveChangesAsync();

			if (model.CurrencyId.HasValue)
			{
				var accountInCurrency = new AccountInCurrency
				{
					FinanceUserId = model.FinanceUserId.Value,
					AccountId = accountEntity.Entity.Id,
					CurrencyId = model.CurrencyId.Value,
				}.CreatedAndModifiedNow();

				var accountInCurrencyEntity = await _dbContext.AddAsync(accountInCurrency);
				await SaveChangesAsync();
			}

			return Ok();
		}

		private async Task<FinanceUser> GetCurrentUser(CancellationToken cancellationToken = default)
		{
			var identityUser = await _userManager.GetUserAsync(User);
			var financeUser =
				await _dbContext.FinanceUsers
					.Include(user => user.Counterparty)
					.SingleAsync(financeUser => financeUser.IdentityUserId == identityUser.Id, cancellationToken);

			return financeUser;
		}

		private async Task SaveChangesAsync(int expectedCount = 1)
		{
			var writtenEntryCount = await _dbContext.SaveChangesAsync();
			Debug.Assert(writtenEntryCount == expectedCount, $"Expected written entry count {expectedCount}; actual {writtenEntryCount}");
		}
	}
}
