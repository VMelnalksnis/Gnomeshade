// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Identity;
using Gnomeshade.Data.Migrations;
using Gnomeshade.Data.PostgreSQL;
using Gnomeshade.Data.Repositories;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

namespace Gnomeshade.Data.Tests.Integration;

/// <summary>Postgres database initializer for integration testing.</summary>
public class PostgresInitializer
{
	private readonly IServiceProvider _serviceProvider;

	/// <summary>Initializes a new instance of the <see cref="PostgresInitializer"/> class.</summary>
	/// <param name="configuration">Configuration containing the connection string for the test database.</param>
	/// <exception cref="ArgumentException">The connection string does not specify the initial database.</exception>
	public PostgresInitializer(IConfiguration configuration)
	{
		var services = new ServiceCollection();
		services
			.AddSingleton(configuration)
			.AddLogging()
			.AddPostgreSQL(configuration)
			.AddRepositories()
			.AddIdentity<ApplicationUser, ApplicationRole>()
			.AddIdentityStores();
		_serviceProvider = services.BuildServiceProvider();
	}

	/// <summary>Creates a new open connection to the integration test database.</summary>
	/// <returns>An open database connection.</returns>
	public async Task<NpgsqlConnection> CreateConnectionAsync()
	{
		var sqlConnection = _serviceProvider.GetRequiredService<NpgsqlConnection>();
		if ((sqlConnection.State & ConnectionState.Open) is not ConnectionState.Open)
		{
			await sqlConnection.OpenAsync();
		}

		return sqlConnection;
	}

	/// <summary>Creates a new database for integration testing.</summary>
	/// <returns>A valid user for testing.</returns>
	public async Task<UserEntity> SetupDatabaseAsync()
	{
		using var scope = _serviceProvider.CreateScope();
		var provider = scope.ServiceProvider;

		await provider.GetRequiredService<IdentityContext>().Database.MigrateAsync();
		provider.GetRequiredService<IDatabaseMigrator>().Migrate();

		var userUnitOfWork = provider.GetRequiredService<UserUnitOfWork>();
		var userId = Guid.NewGuid();
		await userUnitOfWork.CreateUserAsync(new() { Id = userId, FullName = "Test user" });

		var userRepository = provider.GetRequiredService<UserRepository>();
		return (await userRepository.Get()).Single(user => user.Id == userId);
	}
}
