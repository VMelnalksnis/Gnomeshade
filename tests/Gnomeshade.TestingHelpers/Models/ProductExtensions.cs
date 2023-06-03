// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Gnomeshade.WebApi.Models.Products;

namespace Gnomeshade.TestingHelpers.Models;

public static class ProductExtensions
{
	public static ProductCreation ToCreation(this Product product) => new()
	{
		OwnerId = product.OwnerId,
		Name = product.Name,
		CategoryId = product.CategoryId,
		UnitId = product.UnitId,
		Description = product.Description,
		Sku = product.Sku,
	};
}
