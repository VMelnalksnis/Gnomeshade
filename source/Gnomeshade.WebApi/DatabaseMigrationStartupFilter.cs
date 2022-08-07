// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Data.Identity;
using Gnomeshade.Data.Migrations;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Gnomeshade.WebApi;

/// <summary>Performs database migrations during application startup.</summary>
public sealed class DatabaseMigrationStartupFilter : IStartupFilter
{
	private readonly ApplicationDbContext _applicationDbContext;
	private readonly DatabaseMigrator _databaseMigrator;
	private readonly IConfiguration _configuration;

	/// <summary>Initializes a new instance of the <see cref="DatabaseMigrationStartupFilter"/> class.</summary>
	/// <param name="databaseMigrator">Service that performs the database migration.</param>
	/// <param name="configuration">The application configuration.</param>
	public DatabaseMigrationStartupFilter(DatabaseMigrator databaseMigrator, IConfiguration configuration)
	{
		_databaseMigrator = databaseMigrator;
		_configuration = configuration;
		_applicationDbContext = new(configuration);
	}

	/// <inheritdoc />
	public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
	{
		_applicationDbContext.Database.Migrate();
		_databaseMigrator.Migrate(_configuration.GetConnectionString("FinanceDb"));

		return next;
	}
}
