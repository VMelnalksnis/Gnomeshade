// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data;
using System.Threading.Tasks;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Migrations;
using Gnomeshade.Data.PostgreSQL;
using Gnomeshade.Data.Repositories;

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
		services.AddLogging().AddPostgreSQL(configuration).AddRepositories();
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
		_serviceProvider.GetRequiredService<IDatabaseMigrator>().Migrate();

		var sqlConnection = _serviceProvider.GetRequiredService<NpgsqlConnection>();
		await using var transaction = await sqlConnection.OpenAndBeginTransaction();

		var userId = Guid.NewGuid();
		var user = new UserEntity { Id = userId, ModifiedByUserId = userId };

		var userRepository = _serviceProvider.GetRequiredService<UserRepository>();
		var ownerRepository = _serviceProvider.GetRequiredService<OwnerRepository>();
		var ownershipRepository = _serviceProvider.GetRequiredService<OwnershipRepository>();

		await userRepository.AddWithIdAsync(user, transaction);
		await ownerRepository.AddAsync(user.Id, transaction);
		await ownershipRepository.AddDefaultAsync(user.Id, transaction);

		var counterparty = new CounterpartyEntity
		{
			OwnerId = user.Id,
			CreatedByUserId = user.Id,
			ModifiedByUserId = user.Id,
			Name = "Test counterparty",
		};

		var counterpartyId = await _serviceProvider.GetRequiredService<CounterpartyRepository>().AddAsync(counterparty, transaction);
		user.CounterpartyId = counterpartyId;
		await userRepository.UpdateAsync(user, transaction);
		await transaction.CommitAsync();

		return user;
	}
}
