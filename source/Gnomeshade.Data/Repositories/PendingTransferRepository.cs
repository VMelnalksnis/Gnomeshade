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

/// <summary>Persistence store of <see cref="PendingTransferEntity"/>.</summary>
public sealed class PendingTransferRepository : Repository<PendingTransferEntity>
{
	/// <summary>Initializes a new instance of the <see cref="PendingTransferRepository"/> class.</summary>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public PendingTransferRepository(IDbConnection dbConnection)
		: base(dbConnection)
	{
	}

	/// <inheritdoc />
	protected override string DeleteSql => Queries.PendingTransfer.Delete;

	/// <inheritdoc />
	protected override string InsertSql => Queries.PendingTransfer.Insert;

	/// <inheritdoc />
	protected override string SelectSql => Queries.PendingTransfer.Select;

	/// <inheritdoc />
	protected override string UpdateSql => Queries.PendingTransfer.Update;

	/// <inheritdoc />
	protected override string FindSql => "WHERE pending_transfers.id = @id";

	/// <summary>Gets all pending transfers of the specified transaction.</summary>
	/// <param name="transactionId">The id of the transaction for which to get the pending transfers.</param>
	/// <param name="ownerId">The id of the owner of the pending transfers.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>A collection of all pending transfers for the specified transaction.</returns>
	public Task<IEnumerable<PendingTransferEntity>> GetAllAsync(
		Guid transactionId,
		Guid ownerId,
		CancellationToken cancellationToken = default)
	{
		var sql = $"{SelectSql} WHERE pending_transfers.transaction_id = @transactionId AND {AccessSql}";
		var command = new CommandDefinition(sql, new { transactionId, ownerId }, cancellationToken: cancellationToken);
		return GetEntitiesAsync(command);
	}

	/// <summary>Searches for a pending transfer for a specific transaction with the specified id.</summary>
	/// <param name="transactionId">The transaction id for which to get the pending transfer.</param>
	/// <param name="id">The pending transfer id to search by.</param>
	/// <param name="ownerId">The id of the owner of the pending transfer.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>The pending transfer if one exists, otherwise <see langword="null"/>.</returns>
	public Task<PendingTransferEntity?> FindByIdAsync(Guid transactionId, Guid id, Guid ownerId, CancellationToken cancellationToken = default)
	{
		var sql = $"{SelectSql} WHERE pending_transfers.id = @id AND transaction_id = @transactionId AND {AccessSql}";
		var command = new CommandDefinition(sql, new { transactionId, id, ownerId }, cancellationToken: cancellationToken);
		return FindAsync(command);
	}

	/// <summary>Searches for a pending transfer for a specific transaction with the specified id that can be updated.</summary>
	/// <param name="transactionId">The transaction id for which to get the pending transfer.</param>
	/// <param name="id">The pending transfer id to search by.</param>
	/// <param name="ownerId">The id of the owner of the pending transfer.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>The pending transfer if one exists and can be updated, otherwise <see langword="null"/>.</returns>
	public Task<PendingTransferEntity?> FindWriteableByIdAsync(Guid transactionId, Guid id, Guid ownerId, CancellationToken cancellationToken = default)
	{
		var sql = $"{SelectSql} WHERE pending_transfers.id = @id AND transaction_id = @transactionId AND {WriteAccessSql}";
		var command = new CommandDefinition(sql, new { transactionId, id, ownerId }, cancellationToken: cancellationToken);
		return FindAsync(command);
	}

	/// <summary>Searches for a pending transfer with the specified bank reference.</summary>
	/// <param name="bankReference">The bank reference by which to get the pending transfer.</param>
	/// <param name="ownerId">The id of the owner of the pending transfer.</param>
	/// <param name="dbTransaction">The database transaction to use for the query.</param>
	/// <returns>The pending transfer if one exists, otherwise <see langword="null"/>.</returns>
	public Task<PendingTransferEntity?> FindByBankReferenceAsync(string bankReference, Guid ownerId, IDbTransaction dbTransaction)
	{
		var sql = $"{SelectSql} WHERE pending_transfers.bank_reference = @bankReference AND {AccessSql}";
		var command = new CommandDefinition(sql, new { bankReference, ownerId }, dbTransaction);
		return FindAsync(command);
	}
}
