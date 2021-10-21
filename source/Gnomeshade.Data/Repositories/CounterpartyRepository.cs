// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data;

using Gnomeshade.Data.Entities;

namespace Gnomeshade.Data.Repositories
{
	public sealed class CounterpartyRepository : Repository<CounterpartyEntity>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CounterpartyRepository"/> class with a database connection.
		/// </summary>
		/// <param name="dbConnection">The database connection for executing queries.</param>
		public CounterpartyRepository(IDbConnection dbConnection)
			: base(dbConnection)
		{
		}

		/// <inheritdoc />
		protected override string DeleteSql => "DELETE FROM counterparties WHERE id = @id;";

		/// <inheritdoc />
		protected override string InsertSql =>
			"INSERT INTO counterparties (owner_id, created_by_user_id, modified_by_user_id, name, normalized_name) VALUES (@OwnerId, @CreatedByUserId, @ModifiedByUserId, @Name, @NormalizedName) RETURNING id;";

		/// <inheritdoc />
		protected override string SelectSql =>
			"SELECT id, created_at CreatedAt, owner_id OwnerId, created_by_user_id CreatedByUserId, modified_at ModifiedAt, modified_by_user_id ModifiedByUserId, name, normalized_name NormalizedName from counterparties";

		/// <inheritdoc />
		protected override string UpdateSql => throw new NotImplementedException();
	}
}
