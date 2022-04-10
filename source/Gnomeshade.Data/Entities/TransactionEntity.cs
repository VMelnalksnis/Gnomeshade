// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Data.Entities.Abstractions;

namespace Gnomeshade.Data.Entities;

/// <summary>A single financial transaction.</summary>
public sealed record TransactionEntity : IOwnableEntity, IModifiableEntity
{
	/// <inheritdoc/>
	public Guid Id { get; init; }

	/// <inheritdoc/>
	public DateTimeOffset CreatedAt { get; init; }

	/// <inheritdoc/>
	public Guid OwnerId { get; set; }

	/// <inheritdoc/>
	public Guid CreatedByUserId { get; init; }

	/// <inheritdoc/>
	public DateTimeOffset ModifiedAt { get; set; }

	/// <inheritdoc/>
	public Guid ModifiedByUserId { get; set; }

	/// <summary>Gets or sets the point in time when this transaction was posted to an account on the account servicer accounting books.</summary>
	public DateTimeOffset? BookedAt { get; set; }

	/// <summary>Gets or sets the point in time when assets become available in case of deposit, or when assets cease to be available in case of withdrawal.</summary>
	public DateTimeOffset? ValuedAt { get; set; }

	/// <summary>Gets or sets the description of this transaction.</summary>
	public string? Description { get; set; }

	/// <summary>Gets or sets the point in time when this transaction was imported.</summary>
	public DateTimeOffset? ImportedAt { get; set; }

	/// <summary>Gets or sets a hash value of the import source information.</summary>
	public byte[]? ImportHash { get; set; }

	/// <summary>Gets or sets the point in time when this transaction was reconciled.</summary>
	public DateTimeOffset? ReconciledAt { get; set; }

	/// <summary>Gets or sets the id of the user that reconciled the transaction.</summary>
	public Guid? ReconciledByUserId { get; set; }
}
