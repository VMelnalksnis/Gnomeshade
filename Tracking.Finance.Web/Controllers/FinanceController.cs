using System.Diagnostics;
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
		private readonly UserManager<IdentityUser> _userManager;

		/// <summary>
		/// Initializes a new instance of the <see cref="FinanceController"/> class.
		/// </summary>
		/// <param name="dbContext"><see cref="DbContext"/> for accessing and modifying finance data.</param>
		/// <param name="userManager"><see cref="UserManager{TUser}"/> for getting the currently authenticated user.</param>
		protected FinanceController(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
		{
			DbContext = dbContext;
			_userManager = userManager;
		}

		/// <summary>
		/// Gets a <see cref="ApplicationDbContext"/> for accessing and modifying finance data.
		/// </summary>
		protected ApplicationDbContext DbContext { get; }

		/// <summary>
		/// Gets the <see cref="FinanceUser"/> linked to the currently authenticated <see cref="IdentityUser"/>.
		/// </summary>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
		/// <returns><see cref="FinanceUser"/> representing the currently authenticated user.</returns>
		protected async Task<FinanceUser> GetCurrentUser(CancellationToken cancellationToken = default)
		{
			var identityUser = await _userManager.GetUserAsync(User);
			var financeUser =
				await DbContext.FinanceUsers
					.SingleAsync(financeUser => financeUser.IdentityUserId == identityUser.Id, cancellationToken);

			return financeUser;
		}

		/// <summary>
		/// Saves changes made to <see cref="DbContext"/>.
		/// </summary>
		/// <param name="expectedCount">Expected written entry count, used for <see cref="Debug.Assert(bool, string?)"/>.</param>
		/// <returns>A task that represents the asynchronous save operation.</returns>
		protected async Task SaveChangesAsync(int expectedCount = 1)
		{
			var writtenEntryCount = await DbContext.SaveChangesAsync();
			Debug.Assert(writtenEntryCount == expectedCount, $"Expected written entry count {expectedCount}; actual {writtenEntryCount}");
		}
	}
}
