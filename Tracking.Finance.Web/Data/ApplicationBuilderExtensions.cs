using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Tracking.Finance.Web.Data
{
	public static class ApplicationBuilderExtensions
	{
		public static async Task SeedDatabase(this IServiceProvider serviceProvider)
		{
			// By default IdentityUser expects UserName to be an email address
			// If that is not respected, either views or managers/stores need to be modified
			var testUser =
				new IdentityUser
				{
					Email = "foo@bar.com",
					NormalizedEmail = "FOO@BAR.COM",
					EmailConfirmed = true,
					UserName = "foo@bar.com",
					NormalizedUserName = "FOO@BAR.COM",
					PhoneNumber = "+1-202-555-0176",
					PhoneNumberConfirmed = true,
					SecurityStamp = Guid.NewGuid().ToString("D"),
				};

			var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
			var result = await userManager.CreateAsync(testUser, "Password1!");
			if (!result.Succeeded)
			{
				throw new ApplicationException("Failed to seed database");
			}
		}
	}
}
