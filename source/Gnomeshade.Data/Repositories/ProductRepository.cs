// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Data.Common;

using Gnomeshade.Data.Entities;

using Microsoft.Extensions.Logging;

namespace Gnomeshade.Data.Repositories;

/// <summary>Database backed <see cref="ProductEntity"/> repository.</summary>
public sealed class ProductRepository : NamedRepository<ProductEntity>
{
	/// <summary>Initializes a new instance of the <see cref="ProductRepository"/> class with a database connection.</summary>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public ProductRepository(ILogger<ProductRepository> logger, DbConnection dbConnection)
		: base(logger, dbConnection)
	{
	}

	/// <inheritdoc />
	protected override string DeleteSql => Queries.Product.Delete;

	/// <inheritdoc />
	protected override string InsertSql => Queries.Product.Insert;

	/// <inheritdoc />
	protected override string SelectAllSql => Queries.Product.SelectAll;

	/// <inheritdoc />
	protected override string UpdateSql => Queries.Product.Update;

	/// <inheritdoc />
	protected override string FindSql => "p.id = @id";

	protected override string GroupBy => "GROUP BY p.id";

	/// <inheritdoc />
	protected override string NotDeleted => "p.deleted_at IS NULL";

	/// <inheritdoc />
	protected override string NameSql => "p.normalized_name = upper(@name)";

	/// <inheritdoc />
	protected override string SelectSql => Queries.Product.Select;
}
