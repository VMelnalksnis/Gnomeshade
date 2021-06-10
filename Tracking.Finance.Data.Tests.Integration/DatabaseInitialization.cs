// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

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
			createDatabase.CommandText = @"DROP DATABASE IF EXISTS finance_tests; CREATE DATABASE finance_tests;";
			await createDatabase.ExecuteNonQueryAsync();

			await sqlConnection.ChangeDatabaseAsync("finance_tests");

			var cmd = sqlConnection.CreateCommand();
			cmd.CommandText = @"CREATE EXTENSION IF NOT EXISTS ""uuid-ossp"";";
			await cmd.ExecuteNonQueryAsync();

			var createTransactions = sqlConnection.CreateCommand();
			createTransactions.CommandText =
				"CREATE TABLE \"public\".\"transactions\" (\"id\" uuid DEFAULT uuid_generate_v4() NOT NULL, \"owner_id\" uuid NOT NULL, \"created_at\" timestamptz DEFAULT CURRENT_TIMESTAMP NOT NULL, \"created_by_user_id\" uuid NOT NULL, \"modified_at\" timestamptz DEFAULT CURRENT_TIMESTAMP NOT NULL, \"modified_by_user_id\" uuid NOT NULL, \"date\" timestamptz NOT NULL, \"description\" text, \"generated\" boolean NOT NULL, \"validated\" boolean NOT NULL, \"completed\" boolean NOT NULL, CONSTRAINT \"transactions_id\" PRIMARY KEY(\"id\")\n) WITH(oids = false);";
			await createTransactions.ExecuteNonQueryAsync();

			var createTransactionItems = sqlConnection.CreateCommand();
			createTransactionItems.CommandText =
				"CREATE TABLE \"public\".\"transaction_items\" (\"id\" uuid DEFAULT uuid_generate_v4() NOT NULL, \"owner_id\" uuid NOT NULL, \"transaction_id\" uuid NOT NULL, \"source_amount\" numeric NOT NULL, \"source_account_id\" uuid NOT NULL, \"target_amount\" numeric NOT NULL, \"target_account_id\" uuid NOT NULL, \"created_at\" timestamptz DEFAULT CURRENT_TIMESTAMP NOT NULL, \"created_by_user_id\" uuid NOT NULL, \"modified_at\" timestamptz DEFAULT CURRENT_TIMESTAMP NOT NULL, \"modified_by_user_id\" uuid NOT NULL, \"product_id\" uuid NOT NULL, \"amount\" integer NOT NULL, \"bank_reference\" text, \"external_reference\" text, \"internal_reference\" text, \"description\" text, \"delivery_date\" timestamptz, CONSTRAINT \"transaction_items_id\" PRIMARY KEY(\"id\"), CONSTRAINT \"transaction_items_transaction_id_fkey\" FOREIGN KEY(transaction_id) REFERENCES transactions(id) NOT DEFERRABLE\n) WITH(oids = false);";
			await createTransactionItems.ExecuteNonQueryAsync();

			var createOwners = sqlConnection.CreateCommand();
			createOwners.CommandText =
				"DROP TABLE IF EXISTS \"owners\";" +
				"CREATE TABLE \"public\".\"owners\"(\"id\" uuid DEFAULT uuid_generate_v4() NOT NULL, \"created_at\" timestamptz DEFAULT CURRENT_TIMESTAMP NOT NULL, CONSTRAINT \"owners_id\" PRIMARY KEY(\"id\")) WITH(oids = false);";
			await createOwners.ExecuteNonQueryAsync();

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
