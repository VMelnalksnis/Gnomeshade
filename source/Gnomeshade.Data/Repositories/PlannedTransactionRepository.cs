// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data.Common;
using System.Threading.Tasks;

using Dapper;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Logging;

using Microsoft.Extensions.Logging;

namespace Gnomeshade.Data.Repositories;

public sealed class PlannedTransactionRepository(ILogger<PlannedTransactionRepository> logger, DbConnection dbConnection)
	: Repository<PlannedTransactionEntity>(logger, dbConnection)
{
	/// <inheritdoc />
	protected override string DeleteSql => Queries.Transaction.Delete;

	/// <inheritdoc />
	protected override string InsertSql => Queries.Transaction.Insert;

	/// <inheritdoc />
	protected override string SelectSql => Queries.Transaction.Select;

	/// <inheritdoc />
	protected override string TableName => "transactions t";

	/// <inheritdoc />
	protected override string FindSql => "t.id = @id";

	protected override string GroupBy => "GROUP BY t.id";

	/// <inheritdoc />
	protected override string UpdateSql => Queries.Transaction.Update;

	/// <inheritdoc />
	protected override string NotDeleted => "t.deleted_at IS NULL";

	/// <inheritdoc />
	public override Task<Guid> AddAsync(PlannedTransactionEntity entity, DbTransaction dbTransaction)
	{
		Logger.AddingEntityWithTransaction();
		var command = new CommandDefinition(InsertSql, entity, dbTransaction);
		return DbConnection.QuerySingleAsync<Guid>(command);

		// todo Add other planned transactions ?
	}

	/// <inheritdoc />
	public override async Task<int> UpdateAsync(PlannedTransactionEntity entity, DbTransaction dbTransaction)
	{
		Logger.UpdatingEntityWithTransaction();
		var count = await DbConnection.ExecuteAsync(UpdateSql, entity, dbTransaction);
		Logger.UpdatedRows(count);
		return count;

		// todo Update other planned transactions linked to the same schedule ?
	}

	/// <inheritdoc />
	public override async Task<int> DeleteAsync(Guid id, Guid userId, DbTransaction dbTransaction)
	{
		Logger.DeletingEntityWithTransaction(id);
		var count = await DbConnection.ExecuteAsync(DeleteSql, new { id, userId }, dbTransaction);
		Logger.DeletedRows(count);
		return count;
	}
}
