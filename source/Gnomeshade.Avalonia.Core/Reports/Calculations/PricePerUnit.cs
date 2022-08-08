// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;

using LiveChartsCore.Defaults;

namespace Gnomeshade.Avalonia.Core.Reports.Calculations;

/// <summary>Normalizes purchase price by amount of base unit.</summary>
public sealed class PricePerUnit : ICalculationFunction
{
	/// <inheritdoc />
	public string Name => "Price Per Unit";

	decimal ICalculationFunction.Calculate(CalculableValue value)
	{
		return value.Purchase.Price / (value.Purchase.Amount * value.Multiplier);
	}

	IEnumerable<DateTimePoint> ICalculationFunction.Update(IReadOnlyCollection<DateTimePoint> points) => points;
}
