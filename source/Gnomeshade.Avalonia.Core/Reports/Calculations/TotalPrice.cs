// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;

using LiveChartsCore.Defaults;

namespace Gnomeshade.Avalonia.Core.Reports.Calculations;

/// <summary>Calculates the purchase price as is.</summary>
public sealed class TotalPrice : ICalculationFunction
{
	/// <inheritdoc />
	public string Name => "Total Price";

	decimal ICalculationFunction.Calculate(CalculableValue value) => value.Purchase.Price;

	IEnumerable<DateTimePoint> ICalculationFunction.Update(IReadOnlyCollection<DateTimePoint> points) => points;
}
