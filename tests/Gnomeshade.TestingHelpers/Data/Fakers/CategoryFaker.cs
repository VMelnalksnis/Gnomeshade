// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Data.Entities;

namespace Gnomeshade.TestingHelpers.Data.Fakers;

public sealed class CategoryFaker : NamedEntityFaker<CategoryEntity>
{
	public CategoryFaker(Guid userId)
		: base(userId)
	{
		RuleFor(category => category.Name, faker => faker.Commerce.ProductName());
		RuleFor(category => category.NormalizedName, (_, product) => product.Name.ToUpperInvariant());
		RuleFor(category => category.Description, faker => faker.Lorem.Sentence());
	}
}
