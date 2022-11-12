// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Dapper;

using Gnomeshade.Data.Identity;
using Gnomeshade.Data.Migrations;
using Gnomeshade.Data.PostgreSQL.Dapper;
using Gnomeshade.Data.PostgreSQL.Migrations;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NodaTime;

using Npgsql;

namespace Gnomeshade.Data.PostgreSQL;

/// <summary>Extensions methods for configuration.</summary>
public static class ServiceCollectionExtensions
{
	/// <summary>Adds PostgreSQL specific database services.</summary>
	/// <param name="services">The service collection into which to register the services.</param>
	/// <param name="configuration">Configuration from which to get connection strings.</param>
	/// <returns><paramref name="services"/>.</returns>
	public static IServiceCollection AddPostgreSQL(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		SqlMapper.AddTypeHandler(typeof(Instant?), new NullableInstantTypeHandler());
		SqlMapper.AddTypeHandler(new UnsignedIntegerTypeHandler());
		SqlMapper.RemoveTypeMap(typeof(uint));
		SqlMapper.RemoveTypeMap(typeof(uint?));

		return services
			.AddTransient<IDatabaseMigrator, PostgreSQLDatabaseMigrator>()
			.AddNpgsqlDataSource(
				configuration.GetConnectionString("Gnomeshade"),
				builder => builder.UseNodaTime(),
				ServiceLifetime.Scoped);
	}

	/// <summary>Adds <see cref="IdentityContext"/> to service collection.</summary>
	/// <param name="services">The <see cref="IServiceCollection"/> to which to add the context.</param>
	/// <returns>The current <see cref="IdentityBuilder"/>.</returns>
	public static IServiceCollection AddPostgreSQLIdentityContext(this IServiceCollection services) => services
		.AddDbContext<IdentityContext, PostgreSQLIdentityContext>();

	/// <summary>Adds an Entity Framework implementation of identity information stores for PostgreSQL.</summary>
	/// <param name="identityBuilder">The <see cref="IdentityBuilder"/> instance this method extends.</param>
	/// <returns><paramref name="identityBuilder"/> with identity stored added.</returns>
	public static IdentityBuilder AddPostgreSQLIdentity(this IdentityBuilder identityBuilder) => identityBuilder
		.AddEntityFrameworkStores<PostgreSQLIdentityContext>();
}
