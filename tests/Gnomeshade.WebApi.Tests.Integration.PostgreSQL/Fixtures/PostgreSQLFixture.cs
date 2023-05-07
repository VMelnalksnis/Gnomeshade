// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;

using DotNet.Testcontainers.Containers;

using Microsoft.Extensions.Configuration;

using Testcontainers.PostgreSql;

namespace Gnomeshade.WebApi.Tests.Integration.Fixtures;

internal sealed class PostgreSQLFixture : WebserverFixture
{
	private readonly PostgreSqlContainer _databaseContainer;
	private readonly IContainer[] _containers;

	internal PostgreSQLFixture(string version)
	{
		Name = version;

		_databaseContainer = new PostgreSqlBuilder().WithImage($"postgres:{version}").Build();
		_containers = new IContainer[] { _databaseContainer };
	}

	internal override string Name { get; }

	protected override IEnumerable<IContainer> Containers => _containers;

	protected override IConfiguration GetAdditionalConfiguration() => new ConfigurationBuilder()
		.AddInMemoryCollection(new Dictionary<string, string?>
		{
			{ "ConnectionStrings:Gnomeshade", $"{_databaseContainer.GetConnectionString()}; Include Error Detail=true" },
			{ "Database:Provider", "PostgreSQL" },
		})
		.Build();
}
