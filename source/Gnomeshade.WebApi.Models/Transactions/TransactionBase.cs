// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using NodaTime;

namespace Gnomeshade.WebApi.Models.Transactions;

/// <summary>Base model for transactions.</summary>
public abstract record TransactionBase
{
	/// <summary>The id of the transaction.</summary>
	public Guid Id { get; set; }

	/// <summary>The id of the owner of the transaction.</summary>
	public Guid OwnerId { get; set; }

	/// <summary>The point in time when the transaction was created.</summary>
	public Instant CreatedAt { get; set; }

	/// <summary>The id of the user that created this transaction.</summary>
	public Guid CreatedByUserId { get; set; }

	/// <summary>The point in time when the transaction was last modified.</summary>
	public Instant ModifiedAt { get; set; }

	/// <summary>The id of the user that last modified this transaction.</summary>
	public Guid ModifiedByUserId { get; set; }

	/// <summary>Whether this transaction is planned or not.</summary>
	public bool Planned { get; set; } // todo is this needed?
}
