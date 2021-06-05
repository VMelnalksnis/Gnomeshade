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

		[OneTimeSetUp]
		public async Task SetupDatabase()
		{
			NpgsqlLogManager.Provider = new ConsoleLoggingProvider(NpgsqlLogLevel.Debug);
			NpgsqlLogManager.IsParameterLoggingEnabled = true;

			var configurationBuilder = new ConfigurationBuilder().AddUserSecrets<DatabaseInitialization>();
			var configuration = configurationBuilder.Build();

			var connectionString = configuration.GetConnectionString("FinanceDb");
			using var sqlConnection = new NpgsqlConnection(connectionString);
			sqlConnection.Open();

			var createDatabase = sqlConnection.CreateCommand();
			createDatabase.CommandText = @"DROP DATABASE IF EXISTS finance_tests; CREATE DATABASE finance_tests";
			await createDatabase.ExecuteNonQueryAsync();

			await sqlConnection.ChangeDatabaseAsync("finance_tests");

			var createTransactions = sqlConnection.CreateCommand();
			createTransactions.CommandText = @"
CREATE SEQUENCE transactions_id_seq INCREMENT 1 MINVALUE 1 MAXVALUE 2147483647 START 11 CACHE 1;

CREATE TABLE ""public"".""transactions"" (
    ""id"" integer DEFAULT nextval('transactions_id_seq') NOT NULL,
    ""user_id"" integer NOT NULL,
    ""created_at"" timestamptz NOT NULL,
    ""created_by_user_id"" integer NOT NULL,
    ""modified_at"" timestamptz NOT NULL,
    ""modified_by_user_id"" integer NOT NULL,
    ""date"" timestamptz NOT NULL,
    ""description"" text,
    ""generated"" boolean NOT NULL,
    ""validated"" boolean NOT NULL,
    ""completed"" boolean NOT NULL,
    CONSTRAINT ""transactions_id"" PRIMARY KEY(""id"")
) WITH(oids = false);";
			await createTransactions.ExecuteNonQueryAsync();

			var createTransactionItems = sqlConnection.CreateCommand();
			createTransactionItems.CommandText = @"
CREATE SEQUENCE transaction_items_id_seq INCREMENT 1 MINVALUE 1 MAXVALUE 2147483647 START 4 CACHE 1;

CREATE TABLE ""public"".""transaction_items"" (
    ""id"" integer DEFAULT nextval('transaction_items_id_seq') NOT NULL,
    ""user_id"" integer NOT NULL,
    ""source_amount"" numeric NOT NULL,
    ""source_account_id"" integer NOT NULL,
    ""target_amount"" numeric NOT NULL,
    ""target_account_id"" integer NOT NULL,
    ""created_at"" timestamptz NOT NULL,
    ""created_by_user_id"" integer NOT NULL,
    ""modified_at"" timestamptz NOT NULL,
    ""modified_by_user_id"" integer NOT NULL,
    ""bank_reference"" text,
    ""external_reference"" text,
    ""internal_reference"" text,
    ""product_id"" integer NOT NULL,
    ""amount"" integer NOT NULL,
    ""description"" text,
    ""transaction_id"" integer NOT NULL,
    ""delivery_date"" timestamptz,
    CONSTRAINT ""transaction_items_id"" PRIMARY KEY(""id""),
    CONSTRAINT ""transaction_items_transaction_id_fkey"" FOREIGN KEY(transaction_id) REFERENCES transactions(id) NOT DEFERRABLE
) WITH(oids = false);";
			await createTransactionItems.ExecuteNonQueryAsync();

			sqlConnection.Dispose();
		}

		[OneTimeTearDown]
		public async Task DropDatabase()
		{
			var configurationBuilder = new ConfigurationBuilder().AddUserSecrets<DatabaseInitialization>();
			var configuration = configurationBuilder.Build();

			NpgsqlConnection.ClearAllPools();

			var connectionString = configuration.GetConnectionString("FinanceDb");
			using var sqlConnection = new NpgsqlConnection(connectionString);
			sqlConnection.Open();

			var createDatabase = sqlConnection.CreateCommand();
			createDatabase.CommandText = @"DROP DATABASE IF EXISTS finance_tests;";
			await createDatabase.ExecuteNonQueryAsync();
		}
	}
}
