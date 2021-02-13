using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Tracking.Finance.Web.Data.Models;

namespace Tracking.Finance.Web.Data
{
	/// <summary>
	/// Application specific implementation of the <see cref="IdentityDbContext"/> class used for identity.
	/// </summary>
	public class ApplicationDbContext : IdentityDbContext
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
		/// </summary>
		/// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}

		public DbSet<Account> Accounts => Set<Account>();

		public DbSet<AccountInCurrency> AccountsInCurrencies => Set<AccountInCurrency>();

		public DbSet<Currency> Currencies => Set<Currency>();

		public DbSet<FinanceUser> FinanceUsers => Set<FinanceUser>();
	}
}
