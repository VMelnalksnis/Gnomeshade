﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Bogus;

using Tracking.Finance.Data.Models;

namespace Tracking.Finance.Data.TestingHelpers
{
	public sealed class UnitFaker : Faker<Unit>
	{
		public UnitFaker(User user)
			: this(user.Id)
		{
		}

		public UnitFaker(Guid userId)
		{
			RuleFor(unit => unit.OwnerId, userId);
			RuleFor(unit => unit.CreatedByUserId, userId);
			RuleFor(unit => unit.ModifiedByUserId, userId);
			RuleFor(unit => unit.Name, faker => faker.Commerce.ProductMaterial());
			RuleFor(unit => unit.NormalizedName, (_, unit) => unit.Name.ToUpperInvariant());
		}
	}
}
