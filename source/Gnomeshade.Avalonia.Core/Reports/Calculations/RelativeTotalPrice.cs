// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

using LiveChartsCore.Defaults;

namespace Gnomeshade.Avalonia.Core.Reports.Calculations;

/// <summary>Calculates purchase price relative to first value.</summary>
public sealed class RelativeTotalPrice : ICalculationFunction
{
	/// <inheritdoc />
	public string Name => "Total Price (Relative)";

	decimal ICalculationFunction.Calculate(CalculableValue value) => value.Purchase.Price;

	IEnumerable<DateTimePoint> ICalculationFunction.Update(IReadOnlyCollection<DateTimePoint> points)
	{
		var first = points.First().Value;
		return points.Select(point => new DateTimePoint(point.DateTime, point.Value / first));
	}
}
