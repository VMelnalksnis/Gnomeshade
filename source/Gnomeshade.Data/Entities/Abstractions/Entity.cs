// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using NodaTime;

namespace Gnomeshade.Data.Entities.Abstractions;

/// <summary>Base class for all entities.</summary>
public abstract record Entity : IEntity
{
	/// <inheritdoc />
	public Guid Id { get; init; }

	/// <inheritdoc />
	public Instant CreatedAt { get; init; }

	/// <inheritdoc />
	public Guid CreatedByUserId { get; init; }

	/// <inheritdoc />
	public Instant? DeletedAt { get; set; }

	/// <inheritdoc />
	public Guid? DeletedByUserId { get; set; }
}
