// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Data.Entities;

namespace Gnomeshade.TestingHelpers.Data.Fakers;

public sealed class TagFaker : NamedEntityFaker<TagEntity>
{
	public TagFaker(Guid userId)
		: base(userId)
	{
		RuleFor(product => product.Name, faker => faker.Commerce.ProductName());
		RuleFor(product => product.NormalizedName, (_, product) => product.Name.ToUpperInvariant());
		RuleFor(tag => tag.Description, faker => faker.Lorem.Sentence());
	}
}
