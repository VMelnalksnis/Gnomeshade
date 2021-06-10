// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Tracking.Finance.Data.Identity
{
	public static class ConfigurationExtensions
	{
		public static IdentityBuilder AddIdentityContext(this IServiceCollection services, Action<DbContextOptionsBuilder>? optionsAction = null)
		{
			return
				services
					.AddDbContext<ApplicationDbContext>(optionsAction)
					.AddIdentity<ApplicationUser, IdentityRole>()
					.AddEntityFrameworkStores<ApplicationDbContext>()
					.AddDefaultTokenProviders();
		}

		public static void ConfigureIdentityContext(this DbContextOptionsBuilder options, IConfiguration configuration)
		{
			options
				.LogTo(Console.WriteLine)
				.EnableSensitiveDataLogging()
				.UseNpgsql(configuration.GetConnectionString("IdentityDb")); // todo remove dependency on Npgsql
		}
	}
}
