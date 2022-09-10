// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.IO;

using Microsoft.Extensions.Configuration;

namespace Gnomeshade.WebApi.Tests.Integration.Fixtures;

internal sealed class SqliteFixture : WebserverFixture
{
	private const string _databasePath = "gnomeshade.db";

	internal SqliteFixture()
	{
		File.Delete(_databasePath);
	}

	internal override string Name => "SQLite";

	internal override int RedirectPort => 8298;

	protected override IConfiguration GetAdditionalConfiguration() => new ConfigurationBuilder()
		.AddInMemoryCollection(new Dictionary<string, string>
		{
			{ "ConnectionStrings:Gnomeshade", $"Data Source={_databasePath}" },
			{ "Database:Provider", "Sqlite" },
		})
		.Build();
}
