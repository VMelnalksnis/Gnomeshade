// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;

using LiveChartsCore.Defaults;

namespace Gnomeshade.Avalonia.Core.Reports.Calculations;

/// <summary>A function that calculates the value to display for a purchase.</summary>
public interface ICalculationFunction
{
	/// <summary>Gets the name of the calculation.</summary>
	string Name { get; }

	internal decimal Calculate(CalculableValue value);

	internal IEnumerable<DateTimePoint> Update(IReadOnlyCollection<DateTimePoint> points);
}
