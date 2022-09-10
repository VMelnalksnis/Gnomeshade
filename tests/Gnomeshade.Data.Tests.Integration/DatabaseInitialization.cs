// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Threading.Tasks;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

using Gnomeshade.Data.Entities;

using Microsoft.Extensions.Configuration;

using Npgsql;

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

		var configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string>
			{
				{ "ConnectionStrings:Gnomeshade", _postgreSqlTestcontainer.ConnectionString },
			})
			.AddEnvironmentVariables()
			.Build();

		_initializer = new(configuration);
	}

	public static UserEntity TestUser { get; private set; } = null!;

	public static Task<NpgsqlConnection> CreateConnectionAsync() => _initializer.CreateConnectionAsync();

	[OneTimeSetUp]
	public static async Task SetupDatabaseAsync()
	{
		AssertionOptions.AssertEquivalencyUsing(options => options.ComparingByMembers<AccountEntity>());

		TestUser = await _initializer.SetupDatabaseAsync();
	}

	[OneTimeTearDown]
	public static Task DropDatabaseAsync() => _postgreSqlTestcontainer.StopAsync();
}
