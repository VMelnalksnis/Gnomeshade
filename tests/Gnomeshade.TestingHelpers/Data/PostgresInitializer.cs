// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Migrations;
using Gnomeshade.Data.Repositories;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Npgsql;

namespace Gnomeshade.TestingHelpers.Data;

/// <summary>
/// Postgres database initializer for integration testing.
/// </summary>
public class PostgresInitializer
{
	private const string _connectionStringName = "FinanceDb";

	private readonly string _database;
	private readonly DatabaseMigrator _databaseMigrator;

	/// <summary>
	/// Initializes a new instance of the <see cref="PostgresInitializer"/> class.
	/// </summary>
	/// <param name="configuration">Configuration containing the connection string for the test database.</param>
	/// <exception cref="ArgumentException">The connection string does not specify the initial database.</exception>
	public PostgresInitializer(IConfiguration configuration, ILogger<DatabaseMigrator> logger)
	{
		ConnectionString = configuration.GetConnectionString(_connectionStringName);
		var database = new NpgsqlConnectionStringBuilder(ConnectionString).Database;
		if (string.IsNullOrWhiteSpace(database))
		{
			throw new ArgumentException(
				$"The database in connection string {_connectionStringName} cannot be null or whitespace",
				nameof(configuration));
		}

		_database = database;

		_databaseMigrator = new(logger);
	}

	/// <summary>
	/// Gets the connection string for the integration test database.
	/// </summary>
	public string ConnectionString { get; }

	/// <summary>
	/// Creates a new open connection to the integration test database.
	/// </summary>
	/// <returns>An open database connection.</returns>
	public async Task<NpgsqlConnection> CreateConnectionAsync()
	{
		var sqlConnection = new NpgsqlConnection(ConnectionString);
		await sqlConnection.OpenAsync().ConfigureAwait(false);
		return sqlConnection;
	}

	/// <summary>
	/// Creates a new database for integration testing.
	/// </summary>
	/// <returns>A valid user for testing.</returns>
	public async Task<UserEntity> SetupDatabaseAsync()
	{
		var connectionString = new NpgsqlConnectionStringBuilder(ConnectionString)
		{
			Database = "postgres",
			IncludeErrorDetail = true,
		};

		await using var sqlConnection = new NpgsqlConnection(connectionString.ToString());
		await sqlConnection.OpenAsync().ConfigureAwait(false);

		var createDatabase = sqlConnection.CreateCommand();
		createDatabase.CommandText = $"DROP DATABASE IF EXISTS \"{_database}\"; CREATE DATABASE \"{_database}\";";
		await createDatabase.ExecuteNonQueryAsync().ConfigureAwait(false);

		await sqlConnection.ChangeDatabaseAsync(_database).ConfigureAwait(false);
		_databaseMigrator.Migrate(ConnectionString);

		await using var transaction = await sqlConnection.BeginTransactionAsync().ConfigureAwait(false);

		var userId = Guid.NewGuid();
		var user = new UserEntity { Id = userId, ModifiedByUserId = userId };

		var userRepository = new UserRepository(sqlConnection);
		var ownerRepository = new OwnerRepository(sqlConnection);
		var ownershipRepository = new OwnershipRepository(sqlConnection);

		await userRepository.AddWithIdAsync(user, transaction).ConfigureAwait(false);
		await ownerRepository.AddAsync(user.Id, transaction).ConfigureAwait(false);
		await ownershipRepository.AddDefaultAsync(user.Id, transaction).ConfigureAwait(false);

		var counterparty = new CounterpartyEntity
		{
			OwnerId = user.Id,
			CreatedByUserId = user.Id,
			ModifiedByUserId = user.Id,
			Name = "Test counterparty",
			NormalizedName = "TEST COUNTERPARTY",
		};

		var counterpartyId = await new CounterpartyRepository(sqlConnection).AddAsync(counterparty, transaction);
		user.CounterpartyId = counterpartyId;
		await userRepository.UpdateAsync(user, transaction).ConfigureAwait(false);
		await transaction.CommitAsync().ConfigureAwait(false);

		return user;
	}

	/// <summary>
	/// Drops the integration test database.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task DropDatabaseAsync()
	{
		NpgsqlConnection.ClearAllPools();

		await using var sqlConnection = new NpgsqlConnection(ConnectionString);
		await sqlConnection.OpenAsync().ConfigureAwait(false);
		await sqlConnection.ChangeDatabaseAsync("postgres").ConfigureAwait(false);

		var clearConnections = sqlConnection.CreateCommand();
		clearConnections.CommandText =
			$"REVOKE CONNECT ON DATABASE \"{_database}\" FROM public;" +
			$"SELECT pid, pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '{_database}' AND pid <> pg_backend_pid();";
		await clearConnections.ExecuteNonQueryAsync().ConfigureAwait(false);

		var createDatabase = sqlConnection.CreateCommand();
		createDatabase.CommandText = $"DROP DATABASE IF EXISTS \"{_database}\";";
		await createDatabase.ExecuteNonQueryAsync().ConfigureAwait(false);
	}
}
