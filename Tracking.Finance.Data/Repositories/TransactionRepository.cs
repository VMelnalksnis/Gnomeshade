﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Data;
using System.Threading.Tasks;

using Dapper;

using Tracking.Finance.Data.Models;

namespace Tracking.Finance.Data.Repositories
{
	public sealed class TransactionRepository : Repository<Transaction>, IModifiableRepository<Transaction>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TransactionRepository"/> class with a database connection.
		/// </summary>
		public TransactionRepository(IDbConnection dbConnection)
			: base(dbConnection)
		{
		}

		/// <inheritdoc/>
		protected override string TableName => "public.\"transactions\"";

		/// <inheritdoc/>
		protected override string ColumnNames => "id Id, owner_id OwnerId, created_at CreatedAt, created_by_user_id CreatedByUserId, " +
			"modified_at ModifiedAt, modified_by_user_id ModifiedByUserId, date Date, " +
			"description Description, generated \"Generated\", validated Validated, completed Completed";

		/// <inheritdoc/>
		protected override string InsertSql => @$"
INSERT INTO {TableName}
	(owner_id, created_by_user_id, modified_by_user_id, date, description, generated, validated, completed)
VALUES
	(@OwnerId, @CreatedByUserId, @ModifiedByUserId, @Date, @Description, @Generated, @Validated, @Completed)
RETURNING id";

		/// <inheritdoc/>
		public async Task UpdateAsync(Transaction entity)
		{
			var sql = $@"
				UPDATE {TableName} 
				SET 
					owner_id = @OwnerId, 
					created_at = @CreatedAt, 
					created_by_user_id = @CreatedByUserId,
					modified_at = @ModifiedAt,
					modified_by_user_id = @ModifiedByUserId,
					date = @Date,
					description = @Description,
					generated = @Generated,
					validated = @Validated,
					completed = @Completed
				WHERE id = @Id";

			await DbConnection.ExecuteAsync(sql, entity);
		}
	}
}
