// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Data.Entities.Abstractions;

using NodaTime;

namespace Gnomeshade.Data.Entities;

/// <summary>Base model for transfers.</summary>
public abstract record TransferBase : Entity, IOwnableEntity, IModifiableEntity, ISortableEntity
{
	/// <inheritdoc />
	public Guid OwnerId { get; set; }

	/// <inheritdoc />
	public Instant ModifiedAt { get; set; }

	/// <inheritdoc />
	public Guid ModifiedByUserId { get; set; }

	/// <inheritdoc />
	public uint? Order { get; set; }

	/// <summary>Gets or sets the id of transaction this transfer is a part of.</summary>
	/// <seealso cref="TransactionEntity"/>
	public Guid TransactionId { get; set; }

	/// <summary>Gets or sets the amount withdrawn from the source account.</summary>
	public decimal SourceAmount { get; set; }

	/// <summary>Gets or sets the amount deposited in the target account.</summary>
	public decimal TargetAmount { get; set; }

	/// <summary>Gets or sets the point in time when this transfer was posted to an account on the account servicer accounting books.</summary>
	public Instant? BookedAt { get; set; } // todo BookedAt or ValuedAt in the base class?

	/// <summary>Gets or sets a value indicating whether the transfer is planned or not.</summary>
	public bool Planned { get; set; } // todo needed?
}
