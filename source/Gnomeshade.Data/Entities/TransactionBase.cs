// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Data.Entities.Abstractions;

using NodaTime;

namespace Gnomeshade.Data.Entities;

/// <summary>Base model for transactions.</summary>
public abstract record TransactionBase : Entity, IOwnableEntity, IModifiableEntity
{
	/// <inheritdoc/>
	public Guid OwnerId { get; set; }

	/// <inheritdoc/>
	public Instant ModifiedAt { get; set; }

	/// <inheritdoc/>
	public Guid ModifiedByUserId { get; set; }

	/// <summary>Gets or sets a value indicating whether the transaction is planned or not.</summary>
	public bool Planned { get; set; }
}
