// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using JetBrains.Annotations;

using NodaTime;

namespace Gnomeshade.WebApi.Models.Products;

/// <summary>A good or a service that can be exchanged during a transaction.</summary>
[PublicAPI]
public sealed record Product
{
	/// <summary>The id of the product.</summary>
	public Guid Id { get; set; }

	/// <summary>The point in time when the product was created.</summary>
	public Instant CreatedAt { get; set; }

	/// <summary>The id of the owner of the product.</summary>
	public Guid OwnerId { get; set; }

	/// <summary>The id of the user that created this product.</summary>
	public Guid CreatedByUserId { get; set; }

	/// <summary>The point in the when the product was last modified.</summary>
	public Instant ModifiedAt { get; set; }

	/// <summary>The id of the user that last modified this product.</summary>
	public Guid ModifiedByUserId { get; set; }

	/// <summary>The name of the product.</summary>
	public string Name { get; set; } = null!;

	/// <summary>The SKU (stock-keeping unit) of the product.</summary>
	public string? Sku { get; set; }

	/// <summary>The description of the product.</summary>
	public string? Description { get; set; }

	/// <summary>The id of the unit in which the amounts of this product are expressed.</summary>
	public Guid? UnitId { get; set; }

	/// <summary>The id of the category of the product.</summary>
	public Guid? CategoryId { get; set; }
}
