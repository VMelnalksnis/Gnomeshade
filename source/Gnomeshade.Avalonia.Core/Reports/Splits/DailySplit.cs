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

internal sealed class DailySplit : IReportSplit
{
	private static readonly LocalDatePattern _pattern = LocalDatePattern.Iso;

	/// <inheritdoc />
	public string Name => "Daily";

	/// <inheritdoc />
	public Axis GetXAxis(ZonedDateTime from, ZonedDateTime to) => new()
	{
		Labels = GetSplits(from, to).Select(date => _pattern.Format(date)).ToArray(),
	};

	/// <inheritdoc />
	public IEnumerable<LocalDate> GetSplits(ZonedDateTime from, ZonedDateTime to)
	{
		var currentDate = new LocalDate(from.Year, from.Month, from.Day);
		while ((currentDate.Year < to.Year) ||
			   (currentDate.Year == to.Year && currentDate.Month < to.Month) ||
			   (currentDate.Year == to.Year && currentDate.Month == to.Month && currentDate.Day <= to.Day))
		{
			yield return currentDate;
			currentDate += Period.FromDays(1);
		}
	}

	/// <inheritdoc />
	public bool Equals(ZonedDateTime x, ZonedDateTime y) =>
		x.Year == y.Year &&
		x.Month == y.Month &&
		x.Day == y.Day;

	/// <inheritdoc />
	public int GetHashCode(ZonedDateTime date) => HashCode.Combine(
		date.Year,
		date.Month,
		date.Day);
}
