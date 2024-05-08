// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using LiveChartsCore.SkiaSharpView;

using NodaTime;
using NodaTime.Text;

namespace Gnomeshade.Avalonia.Core.Reports.Splits;

internal sealed class MonthlySplit : IReportSplit
{
	private static readonly LocalDatePattern _pattern = LocalDatePattern.CreateWithInvariantCulture("uuuu'-'MM");

	/// <inheritdoc />
	public string Name => "Monthly";

	/// <inheritdoc />
	public Axis GetXAxis(ZonedDateTime from, ZonedDateTime to) => new()
	{
		Labels = GetSplits(from, to).Select(date => _pattern.Format(date)).ToArray(),
	};

	/// <inheritdoc />
	public IEnumerable<LocalDate> GetSplits(ZonedDateTime from, ZonedDateTime to)
	{
		var currentDate = new LocalDate(from.Year, from.Month, 1);
		while (currentDate.Year < to.Year || (currentDate.Year == to.Year && currentDate.Month <= to.Month))
		{
			yield return currentDate;
			currentDate += Period.FromMonths(1);
		}
	}

	/// <inheritdoc />
	public bool Equals(ZonedDateTime x, ZonedDateTime y) =>
		x.Year == y.Year &&
		x.Month == y.Month;

	/// <inheritdoc />
	public int GetHashCode(ZonedDateTime date) => HashCode.Combine(
		date.Year,
		date.Month);
}
