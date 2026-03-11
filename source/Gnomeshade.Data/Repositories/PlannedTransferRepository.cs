// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Logging;

using Microsoft.Extensions.Logging;

using static Gnomeshade.Data.Repositories.AccessLevel;

namespace Gnomeshade.Data.Repositories;

/// <summary>Persistence store of <see cref="PlannedTransferEntity"/>.</summary>
public sealed class PlannedTransferRepository(ILogger<PlannedTransferRepository> logger, DbConnection dbConnection)
	: TransactionItemRepository<PlannedTransferEntity>(logger, dbConnection)
{
	/// <inheritdoc />
	protected override string DeleteSql => Queries.PlannedTransfer.Delete;

	/// <inheritdoc />
	protected override string InsertSql => Queries.PlannedTransfer.Insert;

	/// <inheritdoc />
	protected override string TableName => "transfers";

	/// <inheritdoc />
	protected override string UpdateSql => Queries.PlannedTransfer.Update;

	/// <inheritdoc />
	protected override string FindSql => "transfers.id = @id";

	protected override string GroupBy => "GROUP BY transfers.id";

	/// <inheritdoc />
	protected override string NotDeleted => "transfers.deleted_at IS NULL";

	/// <inheritdoc />
	protected override string SelectSql => Queries.PlannedTransfer.Select;

	/// <inheritdoc />
	public override Task<IEnumerable<PlannedTransferEntity>> GetAllAsync(
		Guid transactionId,
		Guid userId,
		CancellationToken cancellationToken = default)
	{
		Logger.GetAll();
		return GetEntitiesAsync(new(
			$"{SelectActiveSql} AND transfers.transaction_id = @transactionId {GroupBy};",
			new { transactionId, userId, access = Read.ToParam() },
			cancellationToken: cancellationToken));
	}
}
