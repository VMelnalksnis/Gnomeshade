// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;

using DbUp.Engine;

using DotNet.Testcontainers.Containers;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Testcontainers.PostgreSql;

namespace Gnomeshade.WebApi.Tests.Integration.Fixtures;

internal sealed class PostgreSQLFixture : WebserverFixture
{
	private readonly PostgreSqlContainer _databaseContainer;
	private readonly IContainer[] _containers;

	internal PostgreSQLFixture(string version)
	{
		Name = version;

		_databaseContainer = new PostgreSqlBuilder()
			.WithImage($"postgres:{version}")
			.WithTmpfsMount("/var/lib/postgresql/data")
			.WithEnvironment("PGDATA", "/var/lib/postgresql/data")
			.WithCommand("-c", "fsync=off")
			.WithCommand("-c", "synchronous_commit=off")
			.WithCommand("-c", "full_page_writes=off")
			.Build();

		_containers = [_databaseContainer];
	}

	/// <inheritdoc />
	internal override string Name { get; }

	/// <inheritdoc />
	protected override IEnumerable<IContainer> Containers => _containers;

	/// <inheritdoc />
	protected override IConfiguration GetAdditionalConfiguration() => new ConfigurationBuilder()
		.AddInMemoryCollection(new Dictionary<string, string?>
		{
			{ "ConnectionStrings:Gnomeshade", $"{_databaseContainer.GetConnectionString()}; Include Error Detail=true" },
			{ "Database:Provider", "PostgreSQL" },
		})
		.Build();

	/// <inheritdoc />
	protected override void ConfigureServices(IServiceCollection services)
	{
		services.AddTransient<IScriptPreprocessor, UnloggedTableScriptPreprocessor>();
	}
}
