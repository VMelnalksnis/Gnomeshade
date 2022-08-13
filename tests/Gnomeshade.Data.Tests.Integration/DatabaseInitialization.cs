// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Threading.Tasks;

using Dapper;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

using Gnomeshade.Data.Dapper;
using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Migrations;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

using NodaTime;

using Npgsql;
using Npgsql.Logging;

namespace Gnomeshade.Data.Tests.Integration;

[SetUpFixture]
public class DatabaseInitialization
{
	private static readonly PostgreSqlTestcontainer _postgreSqlTestcontainer;
	private static readonly PostgresInitializer _initializer;

	static DatabaseInitialization()
	{
		_postgreSqlTestcontainer = new TestcontainersBuilder<PostgreSqlTestcontainer>()
			.WithDatabase(new PostgreSqlTestcontainerConfiguration
			{
				Database = "gnomeshade-test",
				Username = "gnomeshade",
				Password = "foobar",
			})
			.WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
			.Build();

		_postgreSqlTestcontainer.StartAsync().GetAwaiter().GetResult();

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string>
			{
				{ "ConnectionStrings:FinanceDb", _postgreSqlTestcontainer.ConnectionString },
			})
			.AddEnvironmentVariables()
			.Build();

		_initializer = new(configuration, new(new NullLogger<DatabaseMigrator>()));
	}

	public static UserEntity TestUser { get; private set; } = null!;

	public static async Task<NpgsqlConnection> CreateConnectionAsync()
	{
		return await _initializer.CreateConnectionAsync();
	}

	[OneTimeSetUp]
	public static async Task SetupDatabaseAsync()
	{
		AssertionOptions.AssertEquivalencyUsing(options => options.ComparingByMembers<AccountEntity>());
		NpgsqlLogManager.Provider = new ConsoleLoggingProvider(NpgsqlLogLevel.Debug);
		NpgsqlLogManager.IsParameterLoggingEnabled = true;
		NpgsqlConnection.GlobalTypeMapper.UseNodaTime();
		SqlMapper.AddTypeHandler(typeof(Instant?), new NullableInstantTypeHandler());

		TestUser = await _initializer.SetupDatabaseAsync();
	}

	[OneTimeTearDown]
	public static async Task DropDatabaseAsync()
	{
		await _postgreSqlTestcontainer.StopAsync();
	}
}
