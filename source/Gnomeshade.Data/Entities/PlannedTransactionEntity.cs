// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Data.Entities.Abstractions;

using NodaTime;

namespace Gnomeshade.Data.Entities;

/// <summary>A single financial transaction.</summary>
public sealed record PlannedTransactionEntity : Entity, IOwnableEntity, IModifiableEntity
{
	/// <inheritdoc/>
	public Guid OwnerId { get; set; }

	/// <inheritdoc/>
	public Instant ModifiedAt { get; set; }

	/// <inheritdoc/>
	public Guid ModifiedByUserId { get; set; }

	public Instant StartTime { get; set; }

	public Period Period { get; set; }
}
