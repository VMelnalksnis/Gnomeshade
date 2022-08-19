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

/// <summary>Persistence store of <see cref="LoanEntity"/>.</summary>
public sealed class LoanRepository : Repository<LoanEntity>
{
	/// <summary>Initializes a new instance of the <see cref="LoanRepository"/> class.</summary>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public LoanRepository(IDbConnection dbConnection)
		: base(dbConnection)
	{
	}

	/// <inheritdoc />
	protected override string DeleteSql => "CALL delete_loan(@id, @ownerId);";

	/// <inheritdoc />
	protected override string InsertSql => Queries.Loan.Insert;

	/// <inheritdoc />
	protected override string SelectSql => Queries.Loan.Select;

	/// <inheritdoc />
	protected override string UpdateSql => Queries.Loan.Update;

	/// <inheritdoc />
	protected override string NotDeleted => "loans.deleted_at IS NULL";

	/// <inheritdoc />
	protected override string FindSql => "WHERE loans.deleted_at IS NULL AND loans.id = @id";

	/// <summary>Gets all loans of the specified transaction.</summary>
	/// <param name="transactionId">The id of the transaction for which to get the loans.</param>
	/// <param name="ownerId">The id of the owner of the loans.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>A collection of all loans for the specified transaction.</returns>
	public Task<IEnumerable<LoanEntity>> GetAllAsync(
		Guid transactionId,
		Guid ownerId,
		CancellationToken cancellationToken)
	{
		var sql = $"{SelectSql} WHERE loans.deleted_at IS NULL AND loans.transaction_id = @{nameof(transactionId)} AND {AccessSql}";
		var command = new CommandDefinition(sql, new { transactionId, ownerId }, cancellationToken: cancellationToken);
		return GetEntitiesAsync(command);
	}

	/// <summary>Gets all loans of the specified counterparty.</summary>
	/// <param name="counterpartyId">The id of the counterparty for which to get the loans.</param>
	/// <param name="ownerId">The id of the owner of the loans.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>A collection of all loans for the specified counterparty.</returns>
	public Task<IEnumerable<LoanEntity>> GetAllForCounterpartyAsync(
		Guid counterpartyId,
		Guid ownerId,
		CancellationToken cancellationToken)
	{
		var sql = $"{SelectSql} WHERE loans.deleted_at IS NULL AND (loans.issuing_counterparty_id = @{nameof(counterpartyId)} OR loans.receiving_counterparty_id = @{nameof(counterpartyId)}) AND {AccessSql}";
		var command = new CommandDefinition(sql, new { counterpartyId, ownerId }, cancellationToken: cancellationToken);
		return GetEntitiesAsync(command);
	}
}
