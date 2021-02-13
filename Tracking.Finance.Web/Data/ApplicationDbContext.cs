using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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
	}
}
