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

/// <summary>Persistence store of <see cref="LoanEntity"/>.</summary>
[Obsolete]
public sealed class LoanRepository : TransactionItemRepository<LoanEntity>
{
	/// <summary>Initializes a new instance of the <see cref="LoanRepository"/> class.</summary>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public LoanRepository(ILogger<LoanRepository> logger, DbConnection dbConnection)
		: base(logger, dbConnection)
	{
	}

	/// <inheritdoc />
	protected override string DeleteSql => Queries.Loan.Delete;

	/// <inheritdoc />
	protected override string InsertSql => Queries.Loan.Insert;

	/// <inheritdoc />
	protected override string SelectAllSql => Queries.Loan.SelectAll;

	/// <inheritdoc />
	protected override string UpdateSql => Queries.Loan.Update;

	/// <inheritdoc />
	protected override string NotDeleted => "loans.deleted_at IS NULL";

	/// <inheritdoc />
	protected override string FindSql => "loans.id = @id";

	protected override string GroupBy => "GROUP BY loans.id";

	/// <inheritdoc />
	protected override string SelectSql => Queries.Loan.Select;

	/// <inheritdoc />
	public override Task<IEnumerable<LoanEntity>> GetAllAsync(
		Guid transactionId,
		Guid userId,
		CancellationToken cancellationToken = default)
	{
		Logger.GetAll();
		return GetEntitiesAsync(new(
			$"{SelectActiveSql} AND loans.transaction_id = @transactionId {GroupBy};",
			new { transactionId, userId, access = Read.ToParam() },
			cancellationToken: cancellationToken));
	}

	/// <summary>Gets all loans of the specified counterparty.</summary>
	/// <param name="counterpartyId">The id of the counterparty for which to get the loans.</param>
	/// <param name="userId">The id of the user requesting access to the entity.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>A collection of all loans for the specified counterparty.</returns>
	public Task<IEnumerable<LoanEntity>> GetAllForCounterpartyAsync(
		Guid counterpartyId,
		Guid userId,
		CancellationToken cancellationToken)
	{
		Logger.GetAll();
		return GetEntitiesAsync(new(
			$"{SelectActiveSql} AND (loans.issuing_counterparty_id = @counterpartyId OR loans.receiving_counterparty_id = @counterpartyId) {GroupBy};",
			new { counterpartyId, userId, access = Read.ToParam() },
			cancellationToken: cancellationToken));
	}
}
