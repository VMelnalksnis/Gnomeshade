// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Data;
using Gnomeshade.Data.Identity;
using Gnomeshade.WebApi.Configuration.Options;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using static Microsoft.Extensions.Logging.LogLevel;

namespace Gnomeshade.WebApi.Configuration.StartupFilters;

/// <summary>Creates the initial admin user on application startup.</summary>
/// <remarks>
/// Implemented using <see cref="BackgroundService"/>,
/// because <see cref="IStartupFilter"/> does not support async.
/// </remarks>
internal sealed partial class AdminUserStartupFilter : IStartupFilter
{
	private readonly ILogger<AdminUserStartupFilter> _logger;

	public AdminUserStartupFilter(ILogger<AdminUserStartupFilter> logger)
	{
		_logger = logger;
	}

	/// <inheritdoc />
	public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => builder =>
	{
		using (var scope = builder.ApplicationServices.CreateScope())
		{
			var adminOptions = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<AdminOptions>>().Value;
			var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
			var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
			var userUnitOfWork = scope.ServiceProvider.GetRequiredService<UserUnitOfWork>();

			var (identityUser, created) = GetOrCreateIdentityUser(adminOptions, userManager).GetAwaiter().GetResult();

			try
			{
				CreateAndAddRequiredRoles(userManager, roleManager, identityUser).GetAwaiter().GetResult();
				if (created)
				{
					userUnitOfWork.CreateUserAsync(identityUser).GetAwaiter().GetResult();
				}
			}
			catch (Exception)
			{
				userManager.DeleteAsync(identityUser).GetAwaiter().GetResult();
				throw;
			}
		}

		next(builder);
	};

	private async Task<(ApplicationUser IdentityUser, bool Created)> GetOrCreateIdentityUser(
		AdminOptions adminOptions,
		UserManager<ApplicationUser> userManager)
	{
		var identityUser = await userManager.FindByNameAsync(adminOptions.Username);
		if (identityUser is not null)
		{
			AdminUserExists();
			return (identityUser, false);
		}

		var adminUser = new ApplicationUser(adminOptions.Username)
		{
			FullName = adminOptions.Username,
		};

		var creationResult = await userManager.CreateAsync(adminUser, adminOptions.Password);
		if (!creationResult.Succeeded)
		{
			var error = string.Join(", ", creationResult.Errors.Select(error => error.Description));
			FailedToCreateUser(error);
			throw new ApplicationException("Failed to create initial admin user");
		}

		identityUser = await userManager.FindByNameAsync(adminOptions.Username);
		if (identityUser is null)
		{
			throw new InvalidOperationException("Could not find user by name after creating it");
		}

		return (identityUser, true);
	}

	private async Task CreateAndAddRequiredRoles(
		UserManager<ApplicationUser> userManager,
		RoleManager<ApplicationRole> roleManager,
		ApplicationUser identityUser)
	{
		var roles = await userManager.GetRolesAsync(identityUser);
		if (roles.Contains(Roles.Administrator, StringComparer.Ordinal))
		{
			AdminUserHasRoles();
			return;
		}

		var adminRole = await roleManager.FindByNameAsync(Roles.Administrator);
		if (adminRole is null)
		{
			adminRole = new(Roles.Administrator);
			var createRoleResult = await roleManager.CreateAsync(adminRole);
			if (!createRoleResult.Succeeded)
			{
				var error = string.Join(", ", createRoleResult.Errors.Select(error => error.Description));
				throw new ApplicationException(error);
			}
		}

		var roleResult = await userManager.AddToRoleAsync(identityUser, Roles.Administrator);
		if (!roleResult.Succeeded)
		{
			var error = string.Join(", ", roleResult.Errors.Select(error => error.Description));
			FailedToAddRoles(error);
			await userManager.DeleteAsync(identityUser);
			throw new ApplicationException("Failed to add roles to initial admin user");
		}
	}

	[LoggerMessage(EventId = 1, Level = Debug, Message = "Initial admin user already exists")]
	private partial void AdminUserExists();

	[LoggerMessage(EventId = 2, Level = Error, Message = "Failed to create initial admin user; {IdentityError}")]
	private partial void FailedToCreateUser(string identityError);

	[LoggerMessage(EventId = 3, Level = Debug, Message = "Initial admin user already has required roles")]
	private partial void AdminUserHasRoles();

	[LoggerMessage(EventId = 4, Level = Error, Message = "Failed to add roles to initial admin user; {IdentityError}")]
	private partial void FailedToAddRoles(string identityError);
}
