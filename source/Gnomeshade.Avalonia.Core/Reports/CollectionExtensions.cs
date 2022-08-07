// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using NodaTime;

namespace Gnomeshade.Avalonia.Core.Reports;

internal static class CollectionExtensions
{
	internal static TResult? MinOrDefault<TSource, TResult>(
		this IReadOnlyCollection<TSource> source,
		Func<TSource, TResult> selector,
		TResult? defaultResult = default)
	{
		return source.Any() ? source.Min(selector) : defaultResult;
	}

	internal static TResult? MaxOrDefault<TSource, TResult>(
		this IReadOnlyCollection<TSource> source,
		Func<TSource, TResult> selector,
		TResult? defaultResult = default)
	{
		return source.Any() ? source.Max(selector) : defaultResult;
	}

	internal static decimal AverageOrDefault<TSource>(
		this IReadOnlyCollection<TSource> source,
		Func<TSource, decimal> selector,
		decimal defaultResult = 0)
	{
		return source.Any() ? source.Average(selector) : defaultResult;
	}

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
}
