// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Data.Models;

namespace Gnomeshade.Data.TestingHelpers
{
	public sealed class UnitFaker : NamedEntityFaker<Unit>
	{
		public UnitFaker(User user)
			: this(user.Id)
		{
		}

		public UnitFaker(Guid userId)
			: base(userId)
		{
			RuleFor(unit => unit.Name, faker => faker.Commerce.ProductMaterial());
			RuleFor(unit => unit.NormalizedName, (_, unit) => unit.Name.ToUpperInvariant());
		}
	}
}
