// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.WebApi.Models.Products;

/// <summary>A good or a service that can be exchanged during a transaction.</summary>
[PublicAPI]
public sealed record Product
{
	/// <summary>The id of the product.</summary>
	public Guid Id { get; init; }

	/// <summary>The point in time when the product was created.</summary>
	public DateTimeOffset CreatedAt { get; init; }

	/// <summary>The id of the owner of the product.</summary>
	public Guid OwnerId { get; init; }

	/// <summary>The id of the user that created this product.</summary>
	public Guid CreatedByUserId { get; init; }

	/// <summary>The point in the when the product was last modified.</summary>
	public DateTimeOffset ModifiedAt { get; init; }

	/// <summary>The id of the user that last modified this product.</summary>
	public Guid ModifiedByUserId { get; init; }

	/// <summary>The name of the product.</summary>
	public string Name { get; init; } = null!;

	/// <summary>The SKU (stock-keeping unit) of the product.</summary>
	public string? Sku { get; init; }

	/// <summary>The description of the product.</summary>
	public string? Description { get; init; }

	/// <summary>The id of the unit in which the amounts of this product are expressed.</summary>
	public Guid? UnitId { get; init; }
}
