// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.WebApi.Models.Products;

/// <summary>The information needed to create or update a product.</summary>
[PublicAPI]
public sealed record ProductCreationModel
{
	/// <inheritdoc cref="Product.Name"/>
	[Required]
	public string? Name { get; init; }

	/// <inheritdoc cref="Product.Sku"/>
	public string? Sku { get; init; }

	/// <inheritdoc cref="Product.Description"/>
	public string? Description { get; init; }

	/// <inheritdoc cref="Product.UnitId"/>
	public Guid? UnitId { get; init; }

	/// <inheritdoc cref="Product.CategoryId"/>
	public Guid? CategoryId { get; init; }
}
