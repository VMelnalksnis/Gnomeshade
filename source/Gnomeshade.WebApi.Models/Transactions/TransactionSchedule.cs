// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using JetBrains.Annotations;

using NodaTime;

namespace Gnomeshade.WebApi.Models.Transactions;

/// <summary>A schedule for planned transactions.</summary>
/// <seealso cref="PlannedTransaction"/>
[PublicAPI]
public sealed record TransactionSchedule
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

	/// <summary>The name of the planned transaction schedule.</summary>
	public string Name { get; set; } = null!;

	/// <summary>The point in time when the first planned transaction will be booked.</summary>
	public Instant StartingAt { get; set; }

	/// <summary>The period between each repeated planned transaction.</summary>
	public Period Period { get; set; } = null!;

	/// <summary>The number of planned transactions to repeat.</summary>
	public int Count { get; set; }
}
