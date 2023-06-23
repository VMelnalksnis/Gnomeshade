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

	protected override string GroupBy => "GROUP BY purchases.id";

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
}
