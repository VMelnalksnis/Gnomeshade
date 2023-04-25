// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Data.Identity;
using Gnomeshade.Data.Migrations;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Gnomeshade.WebApi.Configuration.StartupFilters;

internal sealed class DatabaseMigrationStartupFilter : IStartupFilter
{
	/// <inheritdoc />
	public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => builder =>
	{
		using (var scope = builder.ApplicationServices.CreateScope())
		{
			var identityContext = scope.ServiceProvider.GetRequiredService<IdentityContext>();
			var databaseMigrator = scope.ServiceProvider.GetRequiredService<IDatabaseMigrator>();

			identityContext.Database.Migrate();
			databaseMigrator.Migrate();
		}

		next(builder);
	};
}
