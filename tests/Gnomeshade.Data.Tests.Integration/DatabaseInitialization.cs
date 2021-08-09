// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.IO;
using System.Threading.Tasks;

using FluentAssertions;

using Gnomeshade.Data.Models;
using Gnomeshade.Data.Repositories;

using Microsoft.Extensions.Configuration;

using Npgsql;
using Npgsql.Logging;

using NUnit.Framework;

namespace Gnomeshade.Data.Tests.Integration
{
	[SetUpFixture]
	public class DatabaseInitialization
	{
		public static readonly string ConnectionString =
			new ConfigurationBuilder()
				.AddUserSecrets<DatabaseInitialization>()
				.AddEnvironmentVariables()
				.Build()
				.GetConnectionString("FinanceDb");

		private static readonly string? _database = new NpgsqlConnectionStringBuilder(ConnectionString).Database;

		public static User TestUser { get; private set; } = null!;

		public static async Task<NpgsqlConnection> CreateConnectionAsync()
		{
			var builder = new ConfigurationBuilder()
				.AddUserSecrets<DatabaseInitialization>()
				.AddEnvironmentVariables();

			var configuration = builder.Build();

			var connectionString = configuration.GetConnectionString("FinanceDb");
			var sqlConnection = new NpgsqlConnection(connectionString);
			sqlConnection.Open();

			return sqlConnection;
		}

		[OneTimeSetUp]
		public static async Task SetupDatabaseAsync()
		{
			AssertionOptions.AssertEquivalencyUsing(options => options.ComparingByMembers<Account>());
			if (NpgsqlLogManager.Provider is null)
			{
				NpgsqlLogManager.Provider = new ConsoleLoggingProvider(NpgsqlLogLevel.Debug);
				NpgsqlLogManager.IsParameterLoggingEnabled = true;
			}

			var connectionString = new NpgsqlConnectionStringBuilder(ConnectionString)
			{
				Database = "postgres",
				IncludeErrorDetails = true,
			};

			await using var sqlConnection = new NpgsqlConnection(connectionString.ToString());
			sqlConnection.Open();

			var createDatabase = sqlConnection.CreateCommand();
			createDatabase.CommandText = $"DROP DATABASE IF EXISTS {_database}; CREATE DATABASE {_database};";
			await createDatabase.ExecuteNonQueryAsync().ConfigureAwait(false);

			await sqlConnection.ChangeDatabaseAsync(_database).ConfigureAwait(false);

			var sqlPath = Path.Combine(Directory.GetCurrentDirectory(), "CreateDatabase.sql");
			var sql = await File.ReadAllTextAsync(sqlPath).ConfigureAwait(false);
			var createTables = sqlConnection.CreateCommand();
			createTables.CommandText = sql;

			await createTables.ExecuteNonQueryAsync().ConfigureAwait(false);

			await using var transaction = await sqlConnection.BeginTransactionAsync().ConfigureAwait(false);
			var userId = Guid.NewGuid();
			TestUser = new() { Id = userId, ModifiedByUserId = userId };

			var userRepository = new UserRepository(sqlConnection);
			var ownerRepository = new OwnerRepository(sqlConnection);
			var ownershipRepository = new OwnershipRepository(sqlConnection);

			await userRepository.AddWithIdAsync(TestUser, transaction).ConfigureAwait(false);
			await ownerRepository.AddAsync(TestUser.Id, transaction).ConfigureAwait(false);
			await ownershipRepository.AddDefaultAsync(TestUser.Id, transaction).ConfigureAwait(false);

			var counterparty = new Counterparty
			{
				OwnerId = userId,
				CreatedByUserId = userId,
				ModifiedByUserId = userId,
				Name = "Test counterparty",
				NormalizedName = "TEST COUNTERPARTY",
			};

			var counterpartyId = await new CounterpartyRepository(sqlConnection).AddAsync(counterparty, transaction);
			await userRepository.AddCounterparty(userId, counterpartyId, transaction);

			await transaction.CommitAsync().ConfigureAwait(false);
		}

		[OneTimeTearDown]
		public static async Task DropDatabaseAsync()
		{
			NpgsqlConnection.ClearAllPools();

			await using var sqlConnection = new NpgsqlConnection(ConnectionString);
			await sqlConnection.OpenAsync().ConfigureAwait(false);
			await sqlConnection.ChangeDatabaseAsync("postgres").ConfigureAwait(false);

			var clearConnections = sqlConnection.CreateCommand();
			clearConnections.CommandText =
				$"REVOKE CONNECT ON DATABASE {_database} FROM public;" +
				$"SELECT pid, pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '{_database}' AND pid <> pg_backend_pid();";
			await clearConnections.ExecuteNonQueryAsync().ConfigureAwait(false);

			var createDatabase = sqlConnection.CreateCommand();
			createDatabase.CommandText = $@"DROP DATABASE IF EXISTS {_database};";
			await createDatabase.ExecuteNonQueryAsync().ConfigureAwait(false);
		}
	}
}
