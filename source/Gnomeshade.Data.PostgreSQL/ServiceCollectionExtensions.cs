// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Data.Common;

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
using Npgsql.Logging;

namespace Gnomeshade.Data.PostgreSQL;

/// <summary>Extensions methods for configuration.</summary>
public static class ServiceCollectionExtensions
{
	private static bool _loggerSet;

	/// <summary>Adds PostgreSQL specific database services.</summary>
	/// <param name="services">The service collection into which to register the services.</param>
	/// <param name="configuration">Configuration from which to get connection strings.</param>
	/// <param name="npgsqlLoggingProvider">The logging provider to use for <see cref="NpgsqlLogManager"/>.</param>
	/// <param name="enableParameterLogging">Whether to log parameter contents, see <see cref="NpgsqlLogManager.IsParameterLoggingEnabled"/>.</param>
	/// <returns><paramref name="services"/>.</returns>
	/// <remarks>This method will throw if <paramref name="npgsqlLoggingProvider"/> is provided and a connection has been opened.</remarks>
	public static IServiceCollection AddPostgreSQL(
		this IServiceCollection services,
		IConfiguration configuration,
		INpgsqlLoggingProvider? npgsqlLoggingProvider = null,
		bool enableParameterLogging = false)
	{
		if (npgsqlLoggingProvider is not null && !_loggerSet)
		{
			NpgsqlLogManager.Provider = npgsqlLoggingProvider;
			_loggerSet = true;
		}

		NpgsqlLogManager.IsParameterLoggingEnabled = enableParameterLogging;
		NpgsqlConnection.GlobalTypeMapper.UseNodaTime();
		SqlMapper.AddTypeHandler(typeof(Instant?), new NullableInstantTypeHandler());
		SqlMapper.AddTypeHandler(new UnsignedIntegerTypeHandler());
		SqlMapper.RemoveTypeMap(typeof(uint));
		SqlMapper.RemoveTypeMap(typeof(uint?));

		return services
			.AddTransient<IDatabaseMigrator, PostgreSQLDatabaseMigrator>()
			.AddScoped<NpgsqlConnection>(_ => new(configuration.GetConnectionString("Gnomeshade")))
			.AddScoped<DbConnection>(provider => provider.GetRequiredService<NpgsqlConnection>());
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
