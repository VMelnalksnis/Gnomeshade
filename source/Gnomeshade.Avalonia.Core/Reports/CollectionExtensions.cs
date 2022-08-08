// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

using Gnomeshade.WebApi.Models.Products;

using NodaTime;

namespace Gnomeshade.Avalonia.Core.Reports;

internal static class CollectionExtensions
{
	internal static List<LocalDate> SplitByMonthUntil(this ZonedDateTime from, ZonedDateTime to)
	{
		var currentDate = new LocalDate(from.Year, from.Month, 1);
		var dates = new List<LocalDate>();
		while (currentDate.Year < to.Year || (currentDate.Year == to.Year && currentDate.Month <= to.Month))
		{
			dates.Add(currentDate);
			currentDate += Period.FromMonths(1);
		}

		return dates;
	}

	internal static Unit? GetBaseUnit(this Unit unit, List<Unit> units, out decimal ratio)
	{
		ratio = 1;
		if (unit.ParentUnitId is null)
		{
			return null;
		}

		while (unit.ParentUnitId is not null)
		{
			ratio /= unit.Multiplier ?? 1;
			unit = units.Single(u => u.Id == unit.ParentUnitId);
		}

		return unit;
	}
}
