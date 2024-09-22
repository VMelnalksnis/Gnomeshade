// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Data.Entities.Abstractions;

using NodaTime;

namespace Gnomeshade.Data.Entities;

/// <summary>A schedule for planned transactions.</summary>
public sealed record TransactionScheduleEntity : Entity, IOwnableEntity, IModifiableEntity, INamedEntity
{
	/// <inheritdoc />
	public Guid OwnerId { get; set; }

	/// <inheritdoc />
	public Instant ModifiedAt { get; set; }

	/// <inheritdoc />
	public Guid ModifiedByUserId { get; set; }

	/// <inheritdoc />
	public string Name { get; set; } = null!;

	/// <inheritdoc />
	public string NormalizedName { get; set; } = null!;

	/// <summary>Gets or sets the start of the planned transaction.</summary>
	public Instant StartingAt { get; set; }

	/// <summary>Gets or sets the period between transactions.</summary>
	public Period Period { get; set; } = null!;

	/// <summary>Gets or sets the number of planned transactions created by the schedule.</summary>
	public int Count { get; set; }
}
