// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Data.Entities;

namespace Gnomeshade.Data.TestingHelpers
{
	public sealed class UnitFaker : NamedEntityFaker<UnitEntity>
	{
		public UnitFaker(UserEntity user)
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
