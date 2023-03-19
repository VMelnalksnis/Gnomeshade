// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data.Common;

using Dapper;

using Gnomeshade.Data.Identity;
using Gnomeshade.Data.Migrations;
using Gnomeshade.Data.Sqlite.Dapper;
using Gnomeshade.Data.Sqlite.Migrations;

using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NodaTime;

namespace Gnomeshade.Data.Sqlite;

/// <summary>Extensions methods for configuration.</summary>
public static class ServiceCollectionExtensions
{
	/// <summary>Adds PostgreSQL specific database services.</summary>
	/// <param name="services">The service collection into which to register the services.</param>
	/// <param name="configuration">Configuration from which to get connection strings.</param>
	/// <returns><paramref name="services"/>.</returns>
	public static IServiceCollection AddSqlite(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		SqlMapper.AddTypeHandler(new GuidHandler());
		SqlMapper.AddTypeHandler(new InstantHandler());
		SqlMapper.AddTypeHandler(new DecimalHandler());

		return services
			.AddTransient<IDatabaseMigrator, SqliteDatabaseMigrator>()
			.AddSingleton(_ =>
			{
				var connection = new SqliteConnection(configuration.GetConnectionString("Gnomeshade"));
				connection.CreateFunction("uuid_generate_v4", Guid.NewGuid);
				connection.CreateFunction("get_system_user_id", () => Guid.Parse("0231c78f-2709-4833-8d8a-aa0704c5b8b6"), true);

				// This is kind of a hack, but this adds the decimal part for seconds without having to write queries for each database
				// AFAIK, the only issue with this is that it does not return the same value within a transaction.
				connection.CreateFunction("CURRENT_TIMESTAMP", () => InstantHandler.Pattern.Format(SystemClock.Instance.GetCurrentInstant()));
				return connection;
			})
			.AddSingleton<DbConnection>(provider => provider.GetRequiredService<SqliteConnection>());
	}

	/// <summary>Adds <see cref="IdentityContext"/> to service collection.</summary>
	/// <param name="services">The <see cref="IServiceCollection"/> to which to add the context.</param>
	/// <returns>The current <see cref="IdentityBuilder"/>.</returns>
	public static IServiceCollection AddSqliteIdentityContext(this IServiceCollection services) => services
		.AddDbContext<IdentityContext, SqliteIdentityContext>();

	/// <summary>Adds an Entity Framework implementation of identity information stores for Sqlite.</summary>
	/// <param name="identityBuilder">The <see cref="IdentityBuilder"/> instance this method extends.</param>
	/// <returns><paramref name="identityBuilder"/> with identity stored added.</returns>
	public static IdentityBuilder AddSqliteIdentity(this IdentityBuilder identityBuilder) => identityBuilder
		.AddEntityFrameworkStores<SqliteIdentityContext>();
}
