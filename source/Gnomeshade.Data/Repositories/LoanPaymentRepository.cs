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

/// <summary>Persistence store of <see cref="LoanPaymentEntity"/>.</summary>
public sealed class LoanPaymentRepository : TransactionItemRepository<LoanPaymentEntity>
{
	/// <summary>Initializes a new instance of the <see cref="LoanPaymentRepository"/> class.</summary>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public LoanPaymentRepository(ILogger<LoanPaymentRepository> logger, DbConnection dbConnection)
		: base(logger, dbConnection)
	{
	}

	/// <inheritdoc />
	protected override string DeleteSql => Queries.LoanPayment.Delete;

	/// <inheritdoc />
	protected override string InsertSql => Queries.LoanPayment.Insert;

	/// <inheritdoc />
	protected override string TableName => "loan_payments";

	/// <inheritdoc />
	protected override string UpdateSql => Queries.LoanPayment.Update;

	/// <inheritdoc />
	protected override string NotDeleted => "loan_payments.deleted_at IS NULL";

	/// <inheritdoc />
	protected override string FindSql => "loan_payments.id = @id";

	protected override string GroupBy => "GROUP BY loan_payments.id";

	/// <inheritdoc />
	protected override string SelectSql => Queries.LoanPayment.Select;

	/// <inheritdoc />
	public override Task<IEnumerable<LoanPaymentEntity>> GetAllAsync(
		Guid transactionId,
		Guid userId,
		CancellationToken cancellationToken = default)
	{
		Logger.GetAll();
		return GetEntitiesAsync(new(
			$"{SelectActiveSql} AND loan_payments.transaction_id = @transactionId {GroupBy};",
			new { transactionId, userId, access = Read.ToParam() },
			cancellationToken: cancellationToken));
	}

	/// <summary>Gets all loans of the specified counterparty.</summary>
	/// <param name="loanId">The id of the counterparty for which to get the loans.</param>
	/// <param name="userId">The id of the user requesting access to the entity.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>A collection of all loans for the specified counterparty.</returns>
	public Task<IEnumerable<LoanPaymentEntity>> GetAllForLoanAsync(
		Guid loanId,
		Guid userId,
		CancellationToken cancellationToken)
	{
		Logger.GetAll();
		return GetEntitiesAsync(new(
			$"{SelectActiveSql} AND (loan_payments.loan_id = @loanId) {GroupBy};",
			new { loanId, userId, access = Read.ToParam() },
			cancellationToken: cancellationToken));
	}

	/// <summary>Gets all loans of the specified counterparty.</summary>
	/// <param name="transactionId">The id of the counterparty for which to get the loans.</param>
	/// <param name="userId">The id of the user requesting access to the entity.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>A collection of all loans for the specified counterparty.</returns>
	public Task<IEnumerable<LoanPaymentEntity>> GetAllForTransactionAsync(
		Guid transactionId,
		Guid userId,
		CancellationToken cancellationToken)
	{
		Logger.GetAll();
		return GetEntitiesAsync(new(
			$"{SelectActiveSql} AND (loan_payments.transaction_id = @transactionId) {GroupBy};",
			new { transactionId, userId, access = Read.ToParam() },
			cancellationToken: cancellationToken));
	}
}
