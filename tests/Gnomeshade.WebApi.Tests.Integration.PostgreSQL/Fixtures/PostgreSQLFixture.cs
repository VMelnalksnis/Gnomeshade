// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

using Microsoft.Extensions.Configuration;

namespace Gnomeshade.WebApi.Tests.Integration.Fixtures;

internal sealed class PostgreSQLFixture : WebserverFixture
{
	private readonly PostgreSqlTestcontainer _databaseContainer;
	private readonly PostgreSqlTestcontainer _identityDatabaseContainer;

	internal PostgreSQLFixture()
	{
		_databaseContainer = new TestcontainersBuilder<PostgreSqlTestcontainer>()
			.WithDatabase(new PostgreSqlTestcontainerConfiguration("postgres:14.5")
			{
				Database = "gnomeshade-test",
				Username = "gnomeshade",
				Password = "foobar",
			})
			.WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
			.Build();

		_identityDatabaseContainer = new TestcontainersBuilder<PostgreSqlTestcontainer>()
			.WithDatabase(new PostgreSqlTestcontainerConfiguration("postgres:14.5")
			{
				Database = "gnomeshade-identity-test",
				Username = "gnomeshade",
				Password = "foobar",
			})
			.WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
			.Build();

		Containers.Add(_databaseContainer);
		Containers.Add(_identityDatabaseContainer);
	}

	internal override string Name => "PostgreSQL";

	internal override int RedirectPort => 8297;

	protected override IConfiguration GetAdditionalConfiguration() => new ConfigurationBuilder()
		.AddInMemoryCollection(new Dictionary<string, string>
		{
			{ "ConnectionStrings:FinanceDb", _databaseContainer.ConnectionString },
			{ "ConnectionStrings:IdentityDb", _identityDatabaseContainer.ConnectionString },
			{ "Database:Provider", "PostgreSQL" },
		})
		.Build();
}
