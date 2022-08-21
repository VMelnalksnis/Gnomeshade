// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Gnomeshade.Data.Identity;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Gnomeshade.Data.PostgreSQL;

/// <inheritdoc />
public sealed class PostgreSQLIdentityContext : IdentityContext
{
	private readonly IConfiguration _configuration;

	/// <summary>Initializes a new instance of the <see cref="PostgreSQLIdentityContext"/> class.</summary>
	/// <param name="configuration">Configuration from which to get connection strings.</param>
	public PostgreSQLIdentityContext(IConfiguration configuration)
	{
		_configuration = configuration;
	}

	/// <inheritdoc />
	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		optionsBuilder.UseNpgsql(_configuration.GetConnectionString(ConnectionStringName));
	}
}
