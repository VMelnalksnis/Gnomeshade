// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Data.Common;

using Gnomeshade.Data.Entities;

using Microsoft.Extensions.Logging;

namespace Gnomeshade.Data.Repositories;

/// <summary>Persistence store of <see cref="LinkEntity"/>.</summary>
public sealed class LinkRepository : Repository<LinkEntity>
{
	/// <summary>Initializes a new instance of the <see cref="LinkRepository"/> class.</summary>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public LinkRepository(ILogger<LinkRepository> logger, DbConnection dbConnection)
		: base(logger, dbConnection)
	{
	}

	/// <inheritdoc />
	protected override string DeleteSql => Queries.Link.Delete;

	/// <inheritdoc />
	protected override string InsertSql => Queries.Link.Insert;

	/// <inheritdoc />
	protected override string SelectAllSql => Queries.Link.SelectAll;

	/// <inheritdoc />
	protected override string UpdateSql => Queries.Link.Update;

	/// <inheritdoc />
	protected override string FindSql => "links.id = @id";

	/// <inheritdoc />
	protected override string NotDeleted => "links.deleted_at IS NULL";

	/// <inheritdoc />
	protected override string SelectSql => Queries.Link.Select;
}
