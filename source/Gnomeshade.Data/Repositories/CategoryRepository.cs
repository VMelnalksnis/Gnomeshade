// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Data;

using Gnomeshade.Data.Entities;

namespace Gnomeshade.Data.Repositories;

/// <summary>Database backed <see cref="CategoryEntity"/> repository.</summary>
public sealed class CategoryRepository : NamedRepository<CategoryEntity>
{
	/// <summary>Initializes a new instance of the <see cref="CategoryRepository"/> class.</summary>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public CategoryRepository(IDbConnection dbConnection)
		: base(dbConnection)
	{
	}

	/// <inheritdoc />
	protected override string DeleteSql => Queries.Category.Delete;

	/// <inheritdoc />
	protected override string InsertSql => Queries.Category.Insert;

	/// <inheritdoc />
	protected override string SelectSql => Queries.Category.Select;

	/// <inheritdoc />
	protected override string UpdateSql => Queries.Category.Update;

	/// <inheritdoc />
	protected override string FindSql => "WHERE c.id = @id";

	/// <inheritdoc />
	protected override string NameSql => "WHERE c.normalized_name = @name";
}
