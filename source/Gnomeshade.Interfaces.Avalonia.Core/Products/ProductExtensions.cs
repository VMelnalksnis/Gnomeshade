// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

using Gnomeshade.Interfaces.WebApi.Models.Products;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.Avalonia.Core.Products;

internal static class ProductExtensions
{
	[LinqTunnel]
	[Pure]
	internal static IEnumerable<ProductOverviewRow> Translate(this IEnumerable<Product> products)
	{
		return products
			.Select(product => new ProductOverviewRow
			{
				Id = product.Id,
				Name = product.Name,
			});
	}
}
