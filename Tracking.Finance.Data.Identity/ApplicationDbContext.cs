using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Tracking.Finance.Data.Identity
{
	public sealed class ApplicationDbContext : IdentityDbContext<ApplicationUser>
	{
		private readonly IConfiguration _configuration;

		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration)
			: base(options)
		{
			_configuration = configuration;
		}

		public ApplicationDbContext(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		/// <inheritdoc/>
		protected sealed override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (optionsBuilder.IsConfigured)
			{
				return;
			}

			optionsBuilder.ConfigureIdentityContext(_configuration);
		}
	}
}
