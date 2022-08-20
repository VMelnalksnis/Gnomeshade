// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Data.Common;

using Gnomeshade.Data.Entities;

namespace Gnomeshade.Data.Repositories;

/// <summary>Database backed <see cref="CategoryEntity"/> repository.</summary>
public sealed class CategoryRepository : NamedRepository<CategoryEntity>
{
	/// <summary>Initializes a new instance of the <see cref="CategoryRepository"/> class.</summary>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public CategoryRepository(DbConnection dbConnection)
		: base(dbConnection)
	{
	}

	/// <inheritdoc />
	protected override string DeleteSql => "CALL delete_category(@id, @ownerId);";

	/// <inheritdoc />
	protected override string InsertSql => Queries.Category.Insert;

	/// <inheritdoc />
	protected override string SelectSql => Queries.Category.Select;

	/// <inheritdoc />
	protected override string UpdateSql => Queries.Category.Update;

	/// <inheritdoc />
	protected override string FindSql => "WHERE c.deleted_at IS NULL AND c.id = @id";

	/// <inheritdoc />
	protected override string NotDeleted => "c.deleted_at IS NULL";

	/// <inheritdoc />
	protected override string NameSql => "WHERE c.deleted_at IS NULL AND c.normalized_name = upper(@name)";
}
