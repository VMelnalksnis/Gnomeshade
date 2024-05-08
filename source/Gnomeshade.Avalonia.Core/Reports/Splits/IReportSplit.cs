// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;

using LiveChartsCore.SkiaSharpView;

using NodaTime;

namespace Gnomeshade.Avalonia.Core.Reports.Splits;

/// <summary>Splits report data in time-based buckets.</summary>
public interface IReportSplit : IEqualityComparer<ZonedDateTime>
{
	/// <summary>Gets the display name of the split.</summary>
	string Name { get; }

	/// <summary>Gets the X axis definition for the specified period.</summary>
	/// <param name="from">The starting point of the axis.</param>
	/// <param name="to">The end point of the axis.</param>
	/// <returns>X Axis definition for the period.</returns>
	Axis GetXAxis(ZonedDateTime from, ZonedDateTime to);

	/// <summary>Gets the date splits for the specified period.</summary>
	/// <param name="from">The minimum date.</param>
	/// <param name="to">The maximum date.</param>
	/// <returns>A collection of all the x points.</returns>
	IEnumerable<LocalDate> GetSplits(ZonedDateTime from, ZonedDateTime to);
}
