// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Gnomeshade.Data.Identity;

/// <summary>Identity database context for <see cref="ApplicationUser"/>.</summary>
public abstract class IdentityContext : IdentityDbContext<ApplicationUser>
{
	/// <summary>The name of the connection string under ConnectionStrings section.</summary>
	protected const string ConnectionStringName = "Gnomeshade";

	/// <summary>The name of the Entity Framework migrations history table.</summary>
	protected const string MigrationHistoryTableName = "__EFMigrationsHistory";

	/// <summary>The name of the schema to use for all identity data.</summary>
	protected const string SchemaName = "identity";

	/// <inheritdoc />
	protected sealed override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);
		builder.HasDefaultSchema(SchemaName);
	}
}
