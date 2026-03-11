// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Dapper;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Logging;
using Gnomeshade.Data.Repositories.Extensions;

using Microsoft.Extensions.Logging;

using static Gnomeshade.Data.Repositories.AccessLevel;

namespace Gnomeshade.Data.Repositories;

/// <summary>Persistence store of <see cref="PlannedPurchaseEntity"/>.</summary>
public sealed class PlannedPurchaseRepository(ILogger<PlannedPurchaseRepository> logger, DbConnection dbConnection)
	: TransactionItemRepository<PlannedPurchaseEntity>(logger, dbConnection)
{
	/// <inheritdoc />
	protected override string DeleteSql => Queries.PlannedPurchase.Delete;

	/// <inheritdoc />
	protected override string InsertSql => Queries.PlannedPurchase.Insert;

	/// <inheritdoc />
	protected override string TableName => "purchases";

	/// <inheritdoc />
	protected override string UpdateSql => Queries.PlannedPurchase.Update;

	/// <inheritdoc />
	protected override string FindSql => "purchases.id = @id";

	protected override string GroupBy => "GROUP BY purchases.id, project_purchases.project_id";

	protected override string NotDeleted => "purchases.deleted_at IS NULL";

	/// <inheritdoc />
	protected override string SelectSql => Queries.PlannedPurchase.Select;

	/// <inheritdoc />
	public override Task<IEnumerable<PlannedPurchaseEntity>> GetAllAsync(
		Guid transactionId,
		Guid userId,
		CancellationToken cancellationToken = default)
	{
		Logger.GetAll();
		return GetEntitiesAsync(new(
			$"{SelectActiveSql} AND purchases.transaction_id = @transactionId {GroupBy};",
			new { transactionId, userId, access = Read.ToParam() },
			cancellationToken: cancellationToken));
	}

	/// <inheritdoc />
	public override Task<IEnumerable<PlannedPurchaseEntity>> GetAllAsync(
		Guid transactionId,
		Guid userId,
		DbTransaction dbTransaction)
	{
		Logger.GetAll();
		return GetEntitiesAsync(new(
			$"{SelectActiveSql} AND purchases.transaction_id = @transactionId {GroupBy};",
			new { transactionId, userId, access = Read.ToParam() },
			dbTransaction));
	}

	/// <inheritdoc />
	protected override async Task<IEnumerable<PlannedPurchaseEntity>> GetEntitiesAsync(CommandDefinition command)
	{
		var purchases = await DbConnection
			.QueryAsync<PlannedPurchaseEntity, IdContainer?, PlannedPurchaseEntity>(
				command,
				(purchase, container) =>
				{
					if (container is not null)
					{
						purchase.ProjectIds = [container.Id];
					}

					return purchase;
				},
				"Id,Id");

		return purchases
			.GroupBy(purchase => purchase.Id)
			.Select(grouping =>
			{
				var purchase = grouping.First();
				purchase.ProjectIds = grouping.SelectMany(entity => entity.ProjectIds).ToList();
				return purchase;
			});
	}
}
