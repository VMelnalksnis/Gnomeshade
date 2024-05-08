// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using LiveChartsCore.SkiaSharpView;

using NodaTime;
using NodaTime.Calendars;

namespace Gnomeshade.Avalonia.Core.Reports.Splits;

internal sealed class WeeklySplit : IReportSplit
{
	private static readonly IWeekYearRule _weekYear = WeekYearRules.Iso;

	/// <inheritdoc />
	public string Name => "Weekly";

	/// <inheritdoc />
	public Axis GetXAxis(ZonedDateTime from, ZonedDateTime to) => new()
	{
		Labels = GetSplits(from, to).Select(date => $"{_weekYear.GetWeekYear(date)}-W{_weekYear.GetWeekOfWeekYear(date)}").ToArray(),
	};

	/// <inheritdoc />
	public IEnumerable<LocalDate> GetSplits(ZonedDateTime from, ZonedDateTime to)
	{
		var currentYear = _weekYear.GetWeekYear(from.Date);
		var currentWeek = _weekYear.GetWeekOfWeekYear(from.Date);

		var endYear = _weekYear.GetWeekYear(to.Date);
		var endWeek = _weekYear.GetWeekOfWeekYear(to.Date);

		while (currentYear < endYear || (currentYear == endYear && currentWeek <= endWeek))
		{
			yield return _weekYear.GetLocalDate(currentYear, currentWeek, IsoDayOfWeek.Monday, from.Calendar);

			if (currentWeek == _weekYear.GetWeeksInWeekYear(currentYear))
			{
				currentYear++;
				currentWeek = 1;
			}
			else
			{
				currentWeek++;
			}
		}
	}

	/// <inheritdoc />
	public bool Equals(ZonedDateTime x, ZonedDateTime y) =>
		_weekYear.GetWeekYear(x.LocalDateTime.Date) == _weekYear.GetWeekYear(y.LocalDateTime.Date) &&
		_weekYear.GetWeekOfWeekYear(x.LocalDateTime.Date) == _weekYear.GetWeekOfWeekYear(y.LocalDateTime.Date);

	/// <inheritdoc />
	public int GetHashCode(ZonedDateTime date) => HashCode.Combine(
		_weekYear.GetWeekYear(date.Date),
		_weekYear.GetWeekOfWeekYear(date.Date));
}
