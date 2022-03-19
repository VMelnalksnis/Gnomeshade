// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Products;

namespace Gnomeshade.Interfaces.Avalonia.Core.Products;

internal static class ProductClientExtensions
{
	internal static async Task<IEnumerable<ProductRow>> GetProductRowsAsync(this IProductClient productClient)
	{
		var products = await productClient.GetProductsAsync().ConfigureAwait(false);
		return await productClient.GetProductRowsAsync(products).ConfigureAwait(false);
	}

	internal static async Task<IEnumerable<ProductRow>> GetProductRowsAsync(
		this IProductClient productClient,
		IEnumerable<Product> products)
	{
		var unitRows = (await productClient.GetUnitRowsAsync().ConfigureAwait(false)).ToList();
		return products.Select(product => new ProductRow(product, unitRows));
	}

	internal static async Task<IEnumerable<UnitRow>> GetUnitRowsAsync(this IProductClient productClient)
	{
		var units = await productClient.GetUnitsAsync().ConfigureAwait(false);
		return units.Select(unit => new UnitRow(unit));
	}
}
