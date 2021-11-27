// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Gnomeshade.Data.Identity;

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
	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		if (optionsBuilder.IsConfigured)
		{
			return;
		}

		optionsBuilder.ConfigureIdentityContext(_configuration);
	}
}
