// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Gnomeshade.Data.Identity;

/// <summary>Extensions methods for configuring <see cref="ApplicationDbContext"/>.</summary>
public static class ConfigurationExtensions
{
	/// <summary>Adds <see cref="ApplicationDbContext"/> to service collection.</summary>
	/// <param name="services">The <see cref="IServiceCollection"/> to which to add the context.</param>
	/// <returns>The current <see cref="IdentityBuilder"/>.</returns>
	public static IdentityBuilder AddIdentityContext(
		this IServiceCollection services)
	{
		return
			services
				.AddDbContext<ApplicationDbContext>()
				.AddIdentity<ApplicationUser, IdentityRole>()
				.AddEntityFrameworkStores<ApplicationDbContext>()
				.AddDefaultTokenProviders();
	}
}
