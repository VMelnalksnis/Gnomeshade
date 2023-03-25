// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Data.Entities.Abstractions;

using NodaTime;

namespace Gnomeshade.Data.Entities;

/// <summary>Keyword for categorizing other entities.</summary>
public sealed record CategoryEntity : Entity, IOwnableEntity, IModifiableEntity, INamedEntity
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

	/// <summary>Gets or sets the description of this category.</summary>
	public string? Description { get; set; }

	/// <summary>Gets or sets the id of the <see cref="CategoryEntity"/> for the category of this category.</summary>
	public Guid? CategoryId { get; set; }

	/// <summary>Gets or sets the id of the <see cref="ProductEntity"/> that represents this category in purchases.</summary>
	public Guid? LinkedProductId { get; set; }
}
