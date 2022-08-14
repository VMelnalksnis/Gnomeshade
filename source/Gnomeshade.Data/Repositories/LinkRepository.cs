// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Data;

using Gnomeshade.Data.Entities;

namespace Gnomeshade.Data.Repositories;

/// <summary>Persistence store of <see cref="LinkEntity"/>.</summary>
public sealed class LinkRepository : Repository<LinkEntity>
{
	/// <summary>Initializes a new instance of the <see cref="LinkRepository"/> class.</summary>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public LinkRepository(IDbConnection dbConnection)
		: base(dbConnection)
	{
	}

	/// <inheritdoc />
	protected override string DeleteSql => "CALL delete_link(@id, @ownerId);";

	/// <inheritdoc />
	protected override string InsertSql => Queries.Link.Insert;

	/// <inheritdoc />
	protected override string SelectSql => Queries.Link.Select;

	/// <inheritdoc />
	protected override string UpdateSql => Queries.Link.Update;

	/// <inheritdoc />
	protected override string FindSql => "WHERE links.deleted_at IS NULL AND links.id = @id";

	/// <inheritdoc />
	protected override string NotDeleted => "links.deleted_at IS NULL";
}
