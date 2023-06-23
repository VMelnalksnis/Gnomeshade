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

/// <summary>Persistence store of <see cref="TransferEntity"/>.</summary>
public sealed class TransferRepository : TransactionItemRepository<TransferEntity>
{
	/// <summary>Initializes a new instance of the <see cref="TransferRepository"/> class.</summary>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public TransferRepository(ILogger<TransferRepository> logger, DbConnection dbConnection)
		: base(logger, dbConnection)
	{
	}

	/// <inheritdoc />
	protected override string DeleteSql => Queries.Transfer.Delete;

	/// <inheritdoc />
	protected override string InsertSql => Queries.Transfer.Insert;

	/// <inheritdoc />
	protected override string SelectAllSql => Queries.Transfer.SelectAll;

	/// <inheritdoc />
	protected override string UpdateSql => Queries.Transfer.Update;

	/// <inheritdoc />
	protected override string FindSql => "transfers.id = @id";

	protected override string GroupBy => "GROUP BY transfers.id";

	/// <inheritdoc />
	protected override string NotDeleted => "transfers.deleted_at IS NULL";

	/// <inheritdoc />
	protected override string SelectSql => Queries.Transfer.Select;

	/// <inheritdoc />
	public override Task<IEnumerable<TransferEntity>> GetAllAsync(
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

	/// <summary>Searches for a transfer with the specified bank reference.</summary>
	/// <param name="bankReference">The bank reference by which to get the transfer.</param>
	/// <param name="userId">The id of the user requesting access to the entity.</param>
	/// <param name="dbTransaction">The database transaction to use for the query.</param>
	/// <returns>The transfer if one exists, otherwise <see langword="null"/>.</returns>
	public Task<TransferEntity?> FindByBankReferenceAsync(
		string bankReference,
		Guid userId,
		DbTransaction dbTransaction)
	{
		Logger.FindBankReferenceWithTransaction(bankReference);
		return FindAsync(new(
			$"{SelectActiveSql} AND transfers.bank_reference = @bankReference {GroupBy};",
			new { bankReference, userId, access = Read.ToParam() },
			dbTransaction));
	}

	public Task<IEnumerable<TransferEntity>> GetByExternalReferenceAsync(
		string externalReference,
		Guid userId,
		DbTransaction dbTransaction)
	{
		return GetEntitiesAsync(new(
			$"{SelectActiveSql} AND transfers.external_reference = @externalReference {GroupBy};",
			new { externalReference, userId, access = Read.ToParam() },
			dbTransaction));
	}
}
