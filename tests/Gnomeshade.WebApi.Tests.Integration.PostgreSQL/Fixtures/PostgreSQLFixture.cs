﻿// Copyright 2021 Valters Melnalksnis
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

	internal PostgreSQLFixture(string version)
	{
		Name = version;
		_databaseContainer = new TestcontainersBuilder<PostgreSqlTestcontainer>()
			.WithDatabase(new PostgreSqlTestcontainerConfiguration($"postgres:{version}")
			{
				Database = "gnomeshade-test",
				Username = "gnomeshade",
				Password = "foobar",
			})
			.WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
			.Build();

		Containers.Add(_databaseContainer);
	}

	internal override string Name { get; }

	internal override int RedirectPort => 8297;

	protected override IConfiguration GetAdditionalConfiguration() => new ConfigurationBuilder()
		.AddInMemoryCollection(new Dictionary<string, string>
		{
			{ "ConnectionStrings:Gnomeshade", _databaseContainer.ConnectionString },
			{ "Database:Provider", "PostgreSQL" },
		})
		.Build();
}