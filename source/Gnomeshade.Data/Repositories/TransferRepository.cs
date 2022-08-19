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

/// <summary>Persistence store of <see cref="TransferEntity"/>.</summary>
public sealed class TransferRepository : TransactionItemRepository<TransferEntity>
{
	/// <summary>Initializes a new instance of the <see cref="TransferRepository"/> class.</summary>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public TransferRepository(IDbConnection dbConnection)
		: base(dbConnection)
	{
	}

	/// <inheritdoc />
	protected override string DeleteSql => "CALL delete_transfer(@id, @ownerId);";

	/// <inheritdoc />
	protected override string InsertSql => Queries.Transfer.Insert;

	/// <inheritdoc />
	protected override string SelectSql => Queries.Transfer.Select;

	/// <inheritdoc />
	protected override string UpdateSql => Queries.Transfer.Update;

	/// <inheritdoc />
	protected override string FindSql => "WHERE transfers.deleted_at IS NULL AND transfers.id = @id";

	/// <inheritdoc />
	protected override string NotDeleted => "transfers.deleted_at IS NULL";

	/// <inheritdoc />
	public override Task<IEnumerable<TransferEntity>> GetAllAsync(
		Guid transactionId,
		Guid ownerId,
		CancellationToken cancellationToken = default)
	{
		var sql = $"{SelectSql} WHERE transfers.deleted_at IS NULL AND transfers.transaction_id = @transactionId AND {AccessSql}";
		var command = new CommandDefinition(sql, new { transactionId, ownerId }, cancellationToken: cancellationToken);
		return GetEntitiesAsync(command);
	}

	/// <summary>Searches for a transfer with the specified bank reference.</summary>
	/// <param name="bankReference">The bank reference by which to get the transfer.</param>
	/// <param name="ownerId">The id of the owner of the transfer.</param>
	/// <param name="dbTransaction">The database transaction to use for the query.</param>
	/// <returns>The transfer if one exists, otherwise <see langword="null"/>.</returns>
	public Task<TransferEntity?> FindByBankReferenceAsync(string bankReference, Guid ownerId, IDbTransaction dbTransaction)
	{
		var sql = $"{SelectSql} WHERE transfers.deleted_at IS NULL AND transfers.bank_reference = @bankReference AND {AccessSql}";
		var command = new CommandDefinition(sql, new { bankReference, ownerId }, dbTransaction);
		return FindAsync(command);
	}
}
