// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Gnomeshade.WebApi.Models.Products;
using Gnomeshade.WebApi.Models.Transactions;

using NodaTime;

namespace Gnomeshade.Avalonia.Core.Reports.Calculations;

internal struct CalculableValue
{
	internal CalculableValue(Purchase purchase, ZonedDateTime date, Unit? unit, decimal multiplier)
	{
		Purchase = purchase;
		Date = date;
		Unit = unit;
		Multiplier = multiplier;
	}

	internal Purchase Purchase { get; }

	internal ZonedDateTime Date { get; }

	internal Unit? Unit { get; }

	internal decimal Multiplier { get; }
}
