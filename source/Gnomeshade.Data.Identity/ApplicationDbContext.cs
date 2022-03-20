// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Gnomeshade.Data.Identity;

/// <summary>Identity database context for <see cref="ApplicationUser"/>.</summary>
public sealed class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
	private readonly IConfiguration _configuration;

	/// <summary>Initializes a new instance of the <see cref="ApplicationDbContext"/> class.</summary>
	/// <param name="configuration">Configuration from which to get the connection string.</param>
	public ApplicationDbContext(IConfiguration configuration)
	{
		_configuration = configuration;
	}

	/// <inheritdoc/>
	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		optionsBuilder.EnableSensitiveDataLogging();
		optionsBuilder.UseNpgsql(_configuration.GetConnectionString("IdentityDb"));
	}
}
