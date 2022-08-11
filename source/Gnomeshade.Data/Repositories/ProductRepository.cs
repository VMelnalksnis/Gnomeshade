// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Data;

using Gnomeshade.Data.Entities;

namespace Gnomeshade.Data.Repositories;

/// <summary>Database backed <see cref="ProductEntity"/> repository.</summary>
public sealed class ProductRepository : NamedRepository<ProductEntity>
{
	/// <summary>Initializes a new instance of the <see cref="ProductRepository"/> class with a database connection.</summary>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public ProductRepository(IDbConnection dbConnection)
		: base(dbConnection)
	{
	}

	/// <inheritdoc />
	protected override string DeleteSql => Queries.Product.Delete;

	/// <inheritdoc />
	protected override string InsertSql => Queries.Product.Insert;

	/// <inheritdoc />
	protected override string SelectSql => Queries.Product.Select;

	/// <inheritdoc />
	protected override string UpdateSql => Queries.Product.Update;

	/// <inheritdoc />
	protected override string FindSql => "WHERE p.id = @id";

	/// <inheritdoc />
	protected override string NameSql => "WHERE p.normalized_name = upper(@name)";
}
