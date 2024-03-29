﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Products;

namespace Gnomeshade.Avalonia.Core.Products;

internal static class ProductClientExtensions
{
	internal static async Task<IEnumerable<ProductRow>> GetProductRowsAsync(this IGnomeshadeClient gnomeshadeClient)
	{
		var products = await gnomeshadeClient.GetProductsAsync();
		return await gnomeshadeClient.GetProductRowsAsync(products);
	}

	internal static async Task<IEnumerable<ProductRow>> GetProductRowsAsync(
		this IGnomeshadeClient gnomeshadeClient,
		IEnumerable<Product> products)
	{
		var unitRows = (await gnomeshadeClient.GetUnitRowsAsync()).ToList();
		var categories = await gnomeshadeClient.GetCategoriesAsync();
		return products.Select(product => new ProductRow(
			product,
			unitRows,
			categories.SingleOrDefault(category => category.Id == product.CategoryId)));
	}

	internal static async Task<IEnumerable<UnitRow>> GetUnitRowsAsync(this IProductClient productClient)
	{
		var units = await productClient.GetUnitsAsync();
		return units.Select(unit => new UnitRow(unit, units));
	}
}
