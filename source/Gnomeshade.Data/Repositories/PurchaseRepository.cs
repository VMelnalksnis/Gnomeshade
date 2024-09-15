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

/// <summary>Persistence store of <see cref="PurchaseEntity"/>.</summary>
public sealed class PurchaseRepository : TransactionItemRepository<PurchaseEntity>
{
	/// <summary>Initializes a new instance of the <see cref="PurchaseRepository"/> class.</summary>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public PurchaseRepository(ILogger<PurchaseRepository> logger, DbConnection dbConnection)
		: base(logger, dbConnection)
	{
	}

	/// <inheritdoc />
	protected override string DeleteSql => Queries.Purchase.Delete;

	/// <inheritdoc />
	protected override string InsertSql => Queries.Purchase.Insert;

	/// <inheritdoc />
	protected override string SelectAllSql => Queries.Purchase.SelectAll;

	/// <inheritdoc />
	protected override string UpdateSql => Queries.Purchase.Update;

	/// <inheritdoc />
	protected override string FindSql => "purchases.id = @id";

	protected override string GroupBy => "GROUP BY purchases.id, project_purchases.project_id";

	protected override string NotDeleted => "purchases.deleted_at IS NULL";

	/// <inheritdoc />
	protected override string SelectSql => Queries.Purchase.Select;

	/// <inheritdoc />
	public override Task<IEnumerable<PurchaseEntity>> GetAllAsync(
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
	public override Task<IEnumerable<PurchaseEntity>> GetAllAsync(
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

	/// <summary>Gets all purchases of the specified product.</summary>
	/// <param name="productId">The id of the product for which to get all purchases.</param>
	/// <param name="userId">The id of the user requesting access to the entity.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>A collection of all purchases of the specified product.</returns>
	public Task<IEnumerable<PurchaseEntity>> GetAllForProduct(
		Guid productId,
		Guid userId,
		CancellationToken cancellationToken)
	{
		Logger.GetAll();
		return GetEntitiesAsync(new(
			$"{SelectActiveSql} AND purchases.product_id = @productId {GroupBy};",
			new { productId, userId, access = Read.ToParam() },
			cancellationToken: cancellationToken));
	}

	/// <summary>Gets all purchases of the specified product.</summary>
	/// <param name="projectId">The id of the project for which to get all purchases.</param>
	/// <param name="userId">The id of the user requesting access to the entity.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>A collection of all purchases of the specified product.</returns>
	public Task<IEnumerable<PurchaseEntity>> GetAllForProject(
		Guid projectId,
		Guid userId,
		CancellationToken cancellationToken)
	{
		Logger.GetAll();
		return GetEntitiesAsync(new(
			$"{SelectActiveSql} AND project_purchases.project_id = @projectId {GroupBy};",
			new { projectId, userId, access = Read.ToParam() },
			cancellationToken: cancellationToken));
	}

	/// <inheritdoc />
	protected override async Task<IEnumerable<PurchaseEntity>> GetEntitiesAsync(CommandDefinition command)
	{
		var purchases = await DbConnection
			.QueryAsync<PurchaseEntity, IdContainer?, PurchaseEntity>(
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
