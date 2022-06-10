// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using JetBrains.Annotations;

using NodaTime;

namespace Gnomeshade.Interfaces.WebApi.Models.Transactions;

/// <summary>A financial transaction during which funds can be transferred between multiple accounts.</summary>
[PublicAPI]
public record Transaction
{
	/// <summary>The id of the transaction.</summary>
	public Guid Id { get; init; }

	/// <summary>The id of the owner of the transaction.</summary>
	public Guid OwnerId { get; init; }

	/// <summary>The point in time when the transaction was created.</summary>
	public Instant CreatedAt { get; init; }

	/// <summary>The id of the user that created this transaction.</summary>
	public Guid CreatedByUserId { get; init; }

	/// <summary>The point in the when the transaction was last modified.</summary>
	public Instant ModifiedAt { get; init; }

	/// <summary>The id of the user that last modified this transaction.</summary>
	public Guid ModifiedByUserId { get; init; }

	/// <summary>The point in time when this transaction was posted to an account on the account servicer accounting books.</summary>
	public Instant? BookedAt { get; init; }

	/// <summary>The point in time when assets become available in case of deposit, or when assets cease to be available in case of withdrawal.</summary>
	public Instant? ValuedAt { get; init; }

	/// <summary>The description of the transaction.</summary>
	public string? Description { get; init; }

	/// <summary>The point in time when this transaction was imported.</summary>
	public Instant? ImportedAt { get; init; }

	/// <summary>Whether or not this transaction was imported.</summary>
	public bool Imported => ImportedAt.HasValue;

	/// <summary>The point in time when this transaction was reconciled.</summary>
	public Instant? ReconciledAt { get; init; }

	/// <summary>Whether or not this transaction was reconciled.</summary>
	public bool Reconciled => ReconciledAt.HasValue;
}
