// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

using Gnomeshade.WebApi.Models.Transactions;

namespace Gnomeshade.Avalonia.Core.Transactions;

/// <summary>Overview of a single <see cref="Transaction"/>.</summary>
public sealed class TransactionOverview : PropertyChangedBase
{
	/// <summary>Initializes a new instance of the <see cref="TransactionOverview"/> class.</summary>
	/// <param name="id">The id of the transactions.</param>
	/// <param name="bookedAt">The point in time when this transaction was posted to an account on the account servicer accounting books.</param>
	/// <param name="valuedAt">The point in time when assets become available in case of deposit, or when assets cease to be available in case of withdrawal.</param>
	/// <param name="reconciledAt">The point in time when this transaction was reconciled.</param>
	/// <param name="transfers">All transfers of the transaction.</param>
	/// <param name="purchases">All purchases of the transaction.</param>
	/// <param name="refunded">Whether the transaction is refunded.</param>
	public TransactionOverview(
		Guid id,
		DateTimeOffset? bookedAt,
		DateTimeOffset? valuedAt,
		DateTimeOffset? reconciledAt,
		List<TransferSummary> transfers,
		List<Purchase> purchases,
		bool refunded)
	{
		Id = id;
		BookedAt = bookedAt;
		ValuedAt = valuedAt;
		ReconciledAt = reconciledAt;
		Transfers = transfers;
		Purchases = purchases;
		Refunded = refunded;
	}

	/// <summary>Gets the id of the transactions.</summary>
	public Guid Id { get; }

	/// <summary>Gets the point in time when this transaction was posted to an account on the account servicer accounting books.</summary>
	public DateTimeOffset? BookedAt { get; }

	/// <summary>Gets the point in time when assets become available in case of deposit, or when assets cease to be available in case of withdrawal.</summary>
	public DateTimeOffset? ValuedAt { get; }

	/// <summary>Gets the date of the transaction.</summary>
	public DateTimeOffset? Date => ValuedAt ?? BookedAt;

	/// <summary>Gets the point in time when this transaction was reconciled.</summary>
	public DateTimeOffset? ReconciledAt { get; }

	/// <summary>Gets a value indicating whether the transaction is reconciled.</summary>
	public bool Reconciled => ReconciledAt is not null;

	/// <summary>Gets all transfers of the transaction.</summary>
	public List<TransferSummary> Transfers { get; }

	/// <summary>Gets all purchases of the transaction.</summary>
	public List<Purchase> Purchases { get; }

	/// <summary>Gets a value indicating whether the transaction is refunded.</summary>
	public bool Refunded { get; }
}
