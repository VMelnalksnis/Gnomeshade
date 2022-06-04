// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Gnomeshade.Interfaces.WebApi.Models.Products;

namespace Gnomeshade.TestingHelpers.Models;

public static class CategoryExtensions
{
	public static CategoryCreation ToCreation(this Category category) => new()
	{
		Name = category.Name,
		CategoryId = category.CategoryId,
		Description = category.Description,
	};
}
