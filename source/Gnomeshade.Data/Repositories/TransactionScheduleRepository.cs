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

/// <summary>Database backed <see cref="ProjectEntity"/> repository.</summary>
public sealed class TransactionScheduleRepository(ILogger<TransactionScheduleRepository> logger, DbConnection dbConnection)
	: NamedRepository<TransactionScheduleEntity>(logger, dbConnection)
{
	/// <inheritdoc />
	protected override string DeleteSql => Queries.TransactionSchedule.Delete;

	/// <inheritdoc />
	protected override string InsertSql => Queries.TransactionSchedule.Insert;

	/// <inheritdoc />
	protected override string TableName => "transaction_schedules";

	/// <inheritdoc />
	protected override string GroupBy => string.Empty;

	/// <inheritdoc />
	protected override string UpdateSql => Queries.TransactionSchedule.Update;

	/// <inheritdoc />
	protected override string FindSql => "transaction_schedules.id = @id";

	/// <inheritdoc />
	protected override string NotDeleted => "schedules.deleted_at IS NULL";

	/// <inheritdoc />
	protected override string NameSql => "schedules.normalized_name = upper(@name)";

	/// <inheritdoc />
	protected override string SelectSql => Queries.TransactionSchedule.Select;

	/// <inheritdoc />
	public override async Task<int> DeleteAsync(Guid id, Guid userId, DbTransaction dbTransaction)
	{
		Logger.DeletingEntityWithTransaction(id);
		var count = await DbConnection.ExecuteAsync(DeleteSql, new { id, userId }, dbTransaction);
		Logger.DeletedRows(count);

		// todo Delete linked planned transactions ?
		return count;
	}
}
