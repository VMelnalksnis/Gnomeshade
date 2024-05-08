// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Data.Entities.Abstractions;

using NodaTime;

namespace Gnomeshade.Data.Entities;

/// <summary>Represents a unit for <see cref="ProductEntity"/> amount.</summary>
public sealed record UnitEntity : Entity, IOwnableEntity, IModifiableEntity, INamedEntity
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

	/// <summary>Gets or sets the symbol of the unit.</summary>
	public string? Symbol { get; set; }

	/// <summary>Gets or sets the id of the parent <see cref="UnitEntity"/>.</summary>
	public Guid? ParentUnitId { get; set; }

	/// <summary>Gets or sets the multiplier to convert a value in this unit to the parent unit.</summary>
	public decimal? Multiplier { get; set; }

	/// <summary>Gets or sets a value indicating whether <see cref="Multiplier"/> needs to be inversed.</summary>
	public bool InverseMultiplier { get; set; }
}
