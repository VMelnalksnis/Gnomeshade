using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Tracking.Finance.Web.Data
{
	public static class DbContextOptionsBuilderExtensions
	{
		public static DbContextOptionsBuilder ConfigurePostgres(this DbContextOptionsBuilder options, IConfiguration configuration)
		{
			return
				options
					.LogTo(Console.WriteLine)
					.EnableSensitiveDataLogging()
					.UseNpgsql(configuration.GetConnectionString(nameof(ApplicationDbContext)))
					.UseSnakeCaseNamingConvention();
		}
	}
}
