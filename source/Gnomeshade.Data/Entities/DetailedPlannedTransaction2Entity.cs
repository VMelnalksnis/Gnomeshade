// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;

namespace Gnomeshade.Data.Entities;

/// <summary>A transaction with all related information.</summary>
public sealed record DetailedPlannedTransaction2Entity : PlannedTransactionEntity
{
	/// <summary>Initializes a new instance of the <see cref="DetailedPlannedTransaction2Entity"/> class.</summary>
	/// <param name="transaction">The transaction from which to initialize.</param>
	public DetailedPlannedTransaction2Entity(PlannedTransactionEntity transaction)
	{
		Id = transaction.Id;
		CreatedAt = transaction.CreatedAt;
		CreatedByUserId = transaction.CreatedByUserId;
		DeletedAt = transaction.DeletedAt;
		DeletedByUserId = transaction.DeletedByUserId;
		OwnerId = transaction.OwnerId;
		ModifiedAt = transaction.ModifiedAt;
		ModifiedByUserId = transaction.ModifiedByUserId;
		ScheduleId = transaction.ScheduleId;
	}

	/// <summary>Gets the transfers of the transaction.</summary>
	public List<PlannedTransferEntity> Transfers { get; init; } = [];

	/// <summary>Gets the purchases of the transaction.</summary>
	public List<PlannedPurchaseEntity> Purchases { get; init; } = [];

	/// <summary>Gets the loan payments associated with the transaction.</summary>
	public List<LoanPaymentEntity> LoanPayments { get; init; } = [];

	/// <summary>Gets the links associated with the transaction.</summary>
	public List<LinkEntity> Links { get; init; } = [];
}
