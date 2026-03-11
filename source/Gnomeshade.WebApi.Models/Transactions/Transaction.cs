// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using JetBrains.Annotations;

using NodaTime;

namespace Gnomeshade.WebApi.Models.Transactions;

/// <summary>A financial transaction during which funds can be transferred between multiple accounts.</summary>
[PublicAPI]
public record Transaction : TransactionBase
{
	/// <summary>The description of the transaction.</summary>
	public string? Description { get; set; }

	/// <summary>The point in time when this transaction was imported.</summary>
	public Instant? ImportedAt { get; set; }

	/// <summary>Whether this transaction was imported.</summary>
	public bool Imported => ImportedAt.HasValue;

	/// <summary>The point in time when this transaction was reconciled.</summary>
	public Instant? ReconciledAt { get; set; }

	/// <summary>Whether this transaction was reconciled.</summary>
	public bool Reconciled => ReconciledAt.HasValue;

	/// <summary>The id of the transaction that refunds this one.</summary>
	public Guid? RefundedBy { get; set; }

	/// <summary>Whether this transaction was refunded.</summary>
	public bool Refunded => RefundedBy.HasValue;
}
