// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Data;

using Gnomeshade.Data.Entities;

namespace Gnomeshade.Data.Repositories
{
	/// <summary>
	/// Database backed <see cref="ProductEntity"/> repository.
	/// </summary>
	public sealed class ProductRepository : NamedRepository<ProductEntity>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ProductRepository"/> class with a database connection.
		/// </summary>
		/// <param name="dbConnection">The database connection for executing queries.</param>
		public ProductRepository(IDbConnection dbConnection)
			: base(dbConnection)
		{
		}

		/// <inheritdoc />
		protected override string DeleteSql => "DELETE FROM products WHERE id = @id;";

		/// <inheritdoc />
		protected override string InsertSql =>
			"INSERT INTO products (owner_id, created_by_user_id, modified_by_user_id, name, normalized_name, description, unit_id) VALUES (@OwnerId, @CreatedByUserId, @ModifiedByUserId, @Name, @NormalizedName, @Description, @UnitId) RETURNING id;";

		/// <inheritdoc />
		protected override string SelectSql =>
			"SELECT id, created_at CreatedAt, owner_id OwnerId, created_by_user_id CreatedByUserId, modified_at ModifiedAt, modified_by_user_id ModifiedByUserId, name, normalized_name NormalizedName, description, unit_id UnitId FROM products";

		/// <inheritdoc />
		protected override string UpdateSql =>
			"UPDATE products SET modified_at = DEFAULT, modified_by_user_id = @ModifiedByUserId, name = @Name, normalized_name = @NormalizedName, description = @Description, unit_id = @UnitId WHERE id = @Id RETURNING id;";
	}
}
