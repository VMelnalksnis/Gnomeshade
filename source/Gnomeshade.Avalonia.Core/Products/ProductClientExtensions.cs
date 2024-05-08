// Copyright 2021 Valters Melnalksnis
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

	internal static async Task<IEnumerable<UnitRow>> GetUnitRowsAsync(this IGnomeshadeClient client)
	{
		var counterparty = await client.GetMyCounterpartyAsync();
		var owners = await client.GetOwnersAsync();
		var defaultOwner = owners.Single(owner => owner.Id == counterparty.Id);

		var units = await client.GetUnitsAsync();
		var rows = units.Select(unit => new UnitRow(unit, units)).ToArray();

		foreach (var row in rows
					 .GroupBy(row => row.Name)
					 .Where(grouping => grouping.Count() > 1)
					 .SelectMany(grouping => grouping)
					 .Where(row => units.Single(unit => unit.Id == row.Id).OwnerId != defaultOwner.Id))
		{
			row.Name = $"{row.Name} ({owners.Single(owner => owner.Id == units.Single(unit => unit.Id == row.Id).OwnerId).Name})";
		}

		return rows;
	}
}
