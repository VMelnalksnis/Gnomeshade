// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using JetBrains.Annotations;

using NodaTime;

namespace Gnomeshade.Interfaces.WebApi.Models.Products;

/// <summary>A keyword that can be assigned to other data, for example, transaction items.</summary>
[PublicAPI]
public sealed record Category
{
	/// <summary>The id of the category.</summary>
	public Guid Id { get; init; }

	/// <summary>The point in time when the category was created.</summary>
	public Instant CreatedAt { get; init; }

	/// <summary>The id of the owner of the category.</summary>
	public Guid OwnerId { get; init; }

	/// <summary>The id of the user that created this category.</summary>
	public Guid CreatedByUserId { get; init; }

	/// <summary>The point in the when the category was last modified.</summary>
	public Instant ModifiedAt { get; init; }

	/// <summary>The id of the user that last modified this category.</summary>
	public Guid ModifiedByUserId { get; init; }

	/// <summary>The name of the category.</summary>
	public string Name { get; init; } = null!;

	/// <summary>The description of the category.</summary>
	public string? Description { get; init; }

	/// <summary>The id of the category to which the category belongs to.</summary>
	public Guid? CategoryId { get; init; }
}
