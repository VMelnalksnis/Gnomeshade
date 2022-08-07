// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Gnomeshade.Interfaces.WebApi.Models.Products;

namespace Gnomeshade.Avalonia.Core.Products;

/// <summary>Overview of a single <see cref="Product"/>.</summary>
public sealed class ProductRow : PropertyChangedBase
{
	/// <summary>Initializes a new instance of the <see cref="ProductRow"/> class.</summary>
	/// <param name="product">The product this row represents.</param>
	/// <param name="unitRows">A collection from which to select the unit of this product.</param>
	/// <param name="category">The category of the product.</param>
	public ProductRow(Product product, IEnumerable<UnitRow> unitRows, Category? category)
	{
		Id = product.Id;
		Name = product.Name;
		Sku = product.Sku;
		Description = product.Description;

		var unit = product.UnitId is null ? null : unitRows.Single(unit => unit.Id == product.UnitId.Value);
		UnitName = unit?.Name;
		UnitId = unit?.Id;
		CategoryName = category?.Name;
		CategoryId = category?.Id;
	}

	/// <inheritdoc cref="Product.Id"/>
	public Guid Id { get; }

	/// <inheritdoc cref="Product.Name"/>
	public string Name { get; }

	/// <inheritdoc cref="Product.Description"/>
	public string? Description { get; }

	/// <inheritdoc cref="Product.Sku"/>
	public string? Sku { get; }

	/// <summary>Gets the name of the unit of the product.</summary>
	public string? UnitName { get; }

	/// <summary>Gets the name of the category of the product.</summary>
	public string? CategoryName { get; }

	internal Guid? UnitId { get; }

	internal Guid? CategoryId { get; }
}
