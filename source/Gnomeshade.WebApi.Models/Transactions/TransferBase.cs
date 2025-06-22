// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using NodaTime;

namespace Gnomeshade.WebApi.Models.Transactions;

/// <summary>Base model for transfers.</summary>
public abstract record TransferBase
{
	/// <summary>The id of the transfer.</summary>
	public Guid Id { get; set; }

	/// <summary>The point in time when the transfer was created.</summary>
	public Instant CreatedAt { get; set; }

	/// <summary>The id of the owner of the transfer.</summary>
	public Guid OwnerId { get; set; }

	/// <summary>The id of the user that created this transfer.</summary>
	public Guid CreatedByUserId { get; set; }

	/// <summary>The point in time when the transfer was last modified.</summary>
	public Instant ModifiedAt { get; set; }

	/// <summary>The id of the user that last modified this transfer.</summary>
	public Guid ModifiedByUserId { get; set; }

	/// <summary>The id of transaction this transfer is a part of.</summary>
	/// <seealso cref="Transaction"/>
	public Guid TransactionId { get; set; }

	/// <summary>The amount withdrawn from the source account.</summary>
	public decimal SourceAmount { get; set; }

	/// <summary>The amount deposited in the target account.</summary>
	public decimal TargetAmount { get; set; }

	/// <summary>The order of the transfer within a transaction.</summary>
	public uint? Order { get; set; }

	/// <summary>The point in time when this transfer was posted to an account on the account servicer accounting books.</summary>
	public Instant? BookedAt { get; set; } // todo LocalTime ?
}
