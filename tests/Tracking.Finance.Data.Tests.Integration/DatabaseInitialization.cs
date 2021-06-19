// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using Npgsql;
using Npgsql.Logging;

using NUnit.Framework;

namespace Tracking.Finance.Data.Tests.Integration
{
	[SetUpFixture]
	public class DatabaseInitialization
	{
		private static readonly string _connectionString =
			new ConfigurationBuilder()
				.AddUserSecrets<DatabaseInitialization>()
				.Build()
				.GetConnectionString("FinanceDb");

		[OneTimeSetUp]
		public async Task SetupDatabase()
		{
			NpgsqlLogManager.Provider = new ConsoleLoggingProvider(NpgsqlLogLevel.Debug);
			NpgsqlLogManager.IsParameterLoggingEnabled = true;

			await using var sqlConnection = new NpgsqlConnection(_connectionString);
			sqlConnection.Open();

			var createDatabase = sqlConnection.CreateCommand();
			createDatabase.CommandText = "DROP DATABASE IF EXISTS finance_tests; CREATE DATABASE finance_tests;";
			await createDatabase.ExecuteNonQueryAsync();

			await sqlConnection.ChangeDatabaseAsync("finance_tests");

			var cmd = sqlConnection.CreateCommand();
			cmd.CommandText = "CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";";
			await cmd.ExecuteNonQueryAsync();

			var sqlPath = Path.Combine(Directory.GetCurrentDirectory(), "CreateDatabase.sql");
			var sql = await File.ReadAllTextAsync(sqlPath);
			var createTables = sqlConnection.CreateCommand();
			createTables.CommandText = sql;

			await createTables.ExecuteNonQueryAsync();
			sqlConnection.Dispose();
		}

		[OneTimeTearDown]
		public async Task DropDatabase()
		{
			NpgsqlConnection.ClearAllPools();

			await using var sqlConnection = new NpgsqlConnection(_connectionString);
			await sqlConnection.OpenAsync();

			var clearConnections = sqlConnection.CreateCommand();
			clearConnections.CommandText =
				"REVOKE CONNECT ON DATABASE finance_tests FROM public;" +
				"SELECT pid, pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = 'finance_tests' AND pid <> pg_backend_pid();";
			await clearConnections.ExecuteNonQueryAsync();

			var createDatabase = sqlConnection.CreateCommand();
			createDatabase.CommandText = @"DROP DATABASE IF EXISTS finance_tests;";
			await createDatabase.ExecuteNonQueryAsync();
		}

		public static async Task<NpgsqlConnection> CreateConnection()
		{
			var builder = new ConfigurationBuilder()
				.AddUserSecrets<DatabaseInitialization>()
				.AddEnvironmentVariables();

			var configuration = builder.Build();

			var connectionString = configuration.GetConnectionString("FinanceDb");
			var sqlConnection = new NpgsqlConnection(connectionString);
			sqlConnection.Open();

			await sqlConnection.ChangeDatabaseAsync("finance_tests");

			return sqlConnection;
		}
	}
}
