// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

using Dapper;

using Gnomeshade.Data.Entities;

namespace Gnomeshade.Data.Repositories;

/// <summary>Persistence store of <see cref="PurchaseEntity"/>.</summary>
public sealed class PurchaseRepository : Repository<PurchaseEntity>
{
	/// <summary>Initializes a new instance of the <see cref="PurchaseRepository"/> class.</summary>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public PurchaseRepository(IDbConnection dbConnection)
		: base(dbConnection)
	{
	}

	/// <inheritdoc />
	protected override string DeleteSql => "CALL delete_purchase(@id, @ownerId);";

	/// <inheritdoc />
	protected override string InsertSql => Queries.Purchase.Insert;

	/// <inheritdoc />
	protected override string SelectSql => Queries.Purchase.Select;

	/// <inheritdoc />
	protected override string UpdateSql => Queries.Purchase.Update;

	/// <inheritdoc />
	protected override string FindSql => "WHERE purchases.deleted_at IS NULL AND purchases.id = @id";

	/// <summary>Gets all purchases of the specified transaction.</summary>
	/// <param name="transactionId">The id of the transaction for which to get the purchases.</param>
	/// <param name="ownerId">The id of the owner of the purchases.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>A collection of all purchases for the specified transaction.</returns>
	public Task<IEnumerable<PurchaseEntity>> GetAllAsync(
		Guid transactionId,
		Guid ownerId,
		CancellationToken cancellationToken = default)
	{
		var sql = $"{SelectSql} WHERE purchases.deleted_at IS NULL AND purchases.transaction_id = @{nameof(transactionId)} AND {AccessSql}";
		var command = new CommandDefinition(sql, new { transactionId, ownerId }, cancellationToken: cancellationToken);
		return GetEntitiesAsync(command);
	}

	/// <summary>Searches for a purchase for a specific transaction with the specified id.</summary>
	/// <param name="transactionId">The transaction id for which to get the purchase.</param>
	/// <param name="id">The purchase id to search by.</param>
	/// <param name="ownerId">The id of the owner of the purchase.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>The purchase if one exists, otherwise <see langword="null"/>.</returns>
	public Task<PurchaseEntity?> FindByIdAsync(Guid transactionId, Guid id, Guid ownerId, CancellationToken cancellationToken = default)
	{
		var sql = $"{SelectSql} WHERE purchases.deleted_at IS NULL AND purchases.id = @id AND transaction_id = @transactionId AND {AccessSql}";
		var command = new CommandDefinition(sql, new { transactionId, id, ownerId }, cancellationToken: cancellationToken);
		return FindAsync(command);
	}

	/// <summary>Searches for a purchase for a specific transaction with the specified id that can be updated.</summary>
	/// <param name="transactionId">The transaction id for which to get the purchase.</param>
	/// <param name="id">The purchase id to search by.</param>
	/// <param name="ownerId">The id of the owner of the purchase.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>The purchase if one exists and can be updated, otherwise <see langword="null"/>.</returns>
	public Task<PurchaseEntity?> FindWriteableByIdAsync(Guid transactionId, Guid id, Guid ownerId, CancellationToken cancellationToken = default)
	{
		var sql = $"{SelectSql} WHERE purchases.deleted_at IS NULL AND purchases.id = @id AND transaction_id = @transactionId AND {WriteAccessSql}";
		var command = new CommandDefinition(sql, new { transactionId, id, ownerId }, cancellationToken: cancellationToken);
		return FindAsync(command);
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
