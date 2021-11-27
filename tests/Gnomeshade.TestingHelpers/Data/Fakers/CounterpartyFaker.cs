// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Data.Entities;

namespace Gnomeshade.TestingHelpers.Data.Fakers;

public sealed class CounterpartyFaker : NamedEntityFaker<CounterpartyEntity>
{
	public CounterpartyFaker(Guid userId)
		: base(userId)
	{
		RuleFor(party => party.Name, faker => faker.Company.CompanyName());
		RuleFor(party => party.NormalizedName, (_, party) => party.Name.ToUpperInvariant());
	}
}
