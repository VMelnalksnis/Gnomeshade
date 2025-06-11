// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data.Common;
using System.Threading.Tasks;

using Dapper;

using Gnomeshade.Data.Entities;

using Microsoft.Extensions.Logging;

namespace Gnomeshade.Data.Repositories;

/// <summary>Database backed <see cref="ProjectEntity"/> repository.</summary>
public sealed class ProjectRepository(ILogger<ProjectRepository> logger, DbConnection dbConnection)
	: NamedRepository<ProjectEntity>(logger, dbConnection)
{
	/// <inheritdoc />
	protected override string DeleteSql => Queries.Project.Delete;

	/// <inheritdoc />
	protected override string InsertSql => Queries.Project.Insert;

	/// <inheritdoc />
	protected override string TableName => "projects";

	/// <inheritdoc />
	protected override string UpdateSql => Queries.Project.Update;

	/// <inheritdoc />
	protected override string FindSql => "projects.id = @id";

	protected override string GroupBy => "GROUP BY projects.id";

	/// <inheritdoc />
	protected override string NotDeleted => "projects.deleted_at IS NULL";

	/// <inheritdoc />
	protected override string NameSql => "projects.normalized_name = upper(@name)";

	/// <inheritdoc />
	protected override string SelectSql => Queries.Project.Select;

	public Task<int> AddPurchaseAsync(Guid id, Guid purchaseId, Guid userId, DbTransaction dbTransaction)
	{
		const string sql =
			"""
			INSERT INTO project_purchases 
				(created_at, created_by_user_id, project_id, purchase_id) 
			VALUES 
				(CURRENT_TIMESTAMP, @userId, @id, @purchaseId);
			""";

		var command = new CommandDefinition(sql, new { id, purchaseId, userId }, dbTransaction);
		return DbConnection.ExecuteAsync(command);
	}

	public Task<int> RemovePurchaseAsync(Guid id, Guid purchaseId, Guid userId, DbTransaction dbTransaction)
	{
		const string sql =
			"""
			DELETE FROM project_purchases
			WHERE project_purchases.project_id = @id
			  AND project_purchases.purchase_id = @purchaseId;
			""";

		var command = new CommandDefinition(sql, new { id, purchaseId, userId }, dbTransaction);
		return DbConnection.ExecuteAsync(command);
	}
}
