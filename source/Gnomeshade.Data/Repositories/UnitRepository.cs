// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Data.Common;

using Gnomeshade.Data.Entities;

namespace Gnomeshade.Data.Repositories;

/// <summary>Database backed <see cref="UnitEntity"/> repository.</summary>
public sealed class UnitRepository : NamedRepository<UnitEntity>
{
	/// <summary>Initializes a new instance of the <see cref="UnitRepository"/> class with a database connection.</summary>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public UnitRepository(DbConnection dbConnection)
		: base(dbConnection)
	{
	}

	/// <inheritdoc />
	protected override string DeleteSql => "CALL delete_unit(@id, @ownerId);";

	/// <inheritdoc />
	protected override string InsertSql => Queries.Unit.Insert;

	/// <inheritdoc />
	protected override string SelectSql => Queries.Unit.Select;

	/// <inheritdoc />
	protected override string UpdateSql => Queries.Unit.Update;

	/// <inheritdoc />
	protected override string FindSql => "WHERE u.deleted_at IS NULL AND u.id = @id";

	/// <inheritdoc />
	protected override string NotDeleted => "u.deleted_at IS NULL";

	/// <inheritdoc />
	protected override string NameSql => "WHERE u.deleted_at IS NULL AND u.normalized_name = upper(@name)";
}
