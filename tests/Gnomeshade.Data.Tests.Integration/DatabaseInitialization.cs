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
		private static readonly string _connectionString =
			new ConfigurationBuilder()
				.AddUserSecrets<DatabaseInitialization>()
				.Build()
				.GetConnectionString("FinanceDb");

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

			await sqlConnection.ChangeDatabaseAsync("finance_tests").ConfigureAwait(false);

			return sqlConnection;
		}

		[OneTimeSetUp]
		public static async Task SetupDatabaseAsync()
		{
			AssertionOptions.AssertEquivalencyUsing(options => options.ComparingByMembers<Account>());

			NpgsqlLogManager.Provider = new ConsoleLoggingProvider(NpgsqlLogLevel.Debug);
			NpgsqlLogManager.IsParameterLoggingEnabled = true;

			await using var sqlConnection = new NpgsqlConnection(_connectionString);
			sqlConnection.Open();

			var createDatabase = sqlConnection.CreateCommand();
			createDatabase.CommandText = "DROP DATABASE IF EXISTS finance_tests; CREATE DATABASE finance_tests;";
			await createDatabase.ExecuteNonQueryAsync().ConfigureAwait(false);

			await sqlConnection.ChangeDatabaseAsync("finance_tests").ConfigureAwait(false);

			var cmd = sqlConnection.CreateCommand();
			cmd.CommandText = "CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";";
			await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);

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

			await transaction.CommitAsync().ConfigureAwait(false);
		}

		[OneTimeTearDown]
		public static async Task DropDatabaseAsync()
		{
			NpgsqlConnection.ClearAllPools();

			await using var sqlConnection = new NpgsqlConnection(_connectionString);
			await sqlConnection.OpenAsync().ConfigureAwait(false);

			var clearConnections = sqlConnection.CreateCommand();
			clearConnections.CommandText =
				"REVOKE CONNECT ON DATABASE finance_tests FROM public;" +
				"SELECT pid, pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = 'finance_tests' AND pid <> pg_backend_pid();";
			await clearConnections.ExecuteNonQueryAsync().ConfigureAwait(false);

			var createDatabase = sqlConnection.CreateCommand();
			createDatabase.CommandText = @"DROP DATABASE IF EXISTS finance_tests;";
			await createDatabase.ExecuteNonQueryAsync().ConfigureAwait(false);
		}
	}
}
