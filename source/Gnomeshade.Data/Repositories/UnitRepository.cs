// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data;

using Gnomeshade.Data.Entities;

namespace Gnomeshade.Data.Repositories
{
	/// <summary>
	/// Database backed <see cref="UnitEntity"/> repository.
	/// </summary>
	public sealed class UnitRepository : Repository<UnitEntity>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="UnitRepository"/> class with a database connection.
		/// </summary>
		/// <param name="dbConnection">The database connection for executing queries.</param>
		public UnitRepository(IDbConnection dbConnection)
			: base(dbConnection)
		{
		}

		/// <inheritdoc />
		protected override string DeleteSql => Queries.Unit.Delete;

		/// <inheritdoc />
		protected override string InsertSql => Queries.Unit.Insert;

		/// <inheritdoc />
		protected override string SelectSql => Queries.Unit.Select;

		/// <inheritdoc />
		protected override string UpdateSql => throw new NotImplementedException();

		/// <inheritdoc />
		protected override string FindSql => "WHERE u.id = @id AND ownerships.user_id = @ownerId;";
	}
}
