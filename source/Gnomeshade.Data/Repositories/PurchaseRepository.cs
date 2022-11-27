// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

using Dapper;

using Gnomeshade.Data.Entities;

namespace Gnomeshade.Data.Repositories;

/// <summary>Persistence store of <see cref="PurchaseEntity"/>.</summary>
public sealed class PurchaseRepository : TransactionItemRepository<PurchaseEntity>
{
	/// <summary>Initializes a new instance of the <see cref="PurchaseRepository"/> class.</summary>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public PurchaseRepository(DbConnection dbConnection)
		: base(dbConnection)
	{
	}

	/// <inheritdoc />
	protected override string DeleteSql => Queries.Purchase.Delete;

	/// <inheritdoc />
	protected override string InsertSql => Queries.Purchase.Insert;

	/// <inheritdoc />
	protected override string SelectSql => Queries.Purchase.Select;

	/// <inheritdoc />
	protected override string UpdateSql => Queries.Purchase.Update;

	/// <inheritdoc />
	protected override string FindSql => "WHERE purchases.deleted_at IS NULL AND purchases.id = @id";

	protected override string NotDeleted => "purchases.deleted_at IS NULL";

	/// <inheritdoc />
	public override Task<IEnumerable<PurchaseEntity>> GetAllAsync(
		Guid transactionId,
		Guid ownerId,
		CancellationToken cancellationToken = default)
	{
		var sql = $"{SelectSql} WHERE purchases.deleted_at IS NULL AND purchases.transaction_id = @{nameof(transactionId)} AND {AccessSql}";
		var command = new CommandDefinition(sql, new { transactionId, ownerId }, cancellationToken: cancellationToken);
		return GetEntitiesAsync(command);
	}

	/// <inheritdoc />
	public override Task<IEnumerable<PurchaseEntity>> GetAllAsync(
		Guid transactionId,
		Guid ownerId,
		IDbTransaction dbTransaction)
	{
		var sql = $"{SelectSql} WHERE purchases.deleted_at IS NULL AND purchases.transaction_id = @{nameof(transactionId)} AND {AccessSql}";
		var command = new CommandDefinition(sql, new { transactionId, ownerId }, dbTransaction);
		return GetEntitiesAsync(command);
	}

	/// <summary>Gets all purchases of the specified product.</summary>
	/// <param name="productId">The id of the product for which to get all purchases.</param>
	/// <param name="ownerId">The id of the owner of the purchases.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>A collection of all purchases of the specified product.</returns>
	public Task<IEnumerable<PurchaseEntity>> GetAllForProduct(
		Guid productId,
		Guid ownerId,
		CancellationToken cancellationToken)
	{
		var sql = $"{SelectSql} WHERE purchases.deleted_at IS NULL AND purchases.product_id = @{nameof(productId)} AND {AccessSql}";
		var command = new CommandDefinition(sql, new { productId, ownerId }, cancellationToken: cancellationToken);
		return GetEntitiesAsync(command);
	}
}
