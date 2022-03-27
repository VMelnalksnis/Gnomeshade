// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

using Dapper;

using Gnomeshade.Data.Entities;

namespace Gnomeshade.Data.Repositories;

/// <summary>Persistence store of <see cref="TransferEntity"/>.</summary>
public sealed class TransferRepository : Repository<TransferEntity>
{
	/// <summary>Initializes a new instance of the <see cref="TransferRepository"/> class.</summary>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public TransferRepository(IDbConnection dbConnection)
		: base(dbConnection)
	{
	}

	/// <inheritdoc />
	protected override string DeleteSql => Queries.Transfer.Delete;

	/// <inheritdoc />
	protected override string InsertSql => Queries.Transfer.Insert;

	/// <inheritdoc />
	protected override string SelectSql => Queries.Transfer.Select;

	/// <inheritdoc />
	protected override string UpdateSql => Queries.Transfer.Update;

	/// <summary>Searches for a transfer for a specific transaction with the specified id.</summary>
	/// <param name="transactionId">The transaction id for which to get the transfer.</param>
	/// <param name="id">The transfer id to search by.</param>
	/// <param name="ownerId">The id of the owner of the transfer.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>The transfer if one exists, otherwise <see langword="null"/>.</returns>
	public Task<TransferEntity?> FindByIdAsync(Guid transactionId, Guid id, Guid ownerId, CancellationToken cancellationToken = default)
	{
		var sql = $"{SelectSql} WHERE transfers.id = @id AND transaction_id = @transactionId AND {_accessSql}";
		var command = new CommandDefinition(sql, new { transactionId, id, ownerId }, cancellationToken: cancellationToken);
		return FindAsync(command);
	}
}
