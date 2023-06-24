// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using LiveChartsCore.SkiaSharpView;

namespace Gnomeshade.Avalonia.Core.Reports;

internal static class DateAxis
{
	private static double DateStep { get; } = TimeSpan.FromDays(30.4375).Ticks;

	internal static Axis GetXAxis() => new() { Labeler = Date, UnitWidth = DateStep, MinStep = DateStep };

	private static string Date(double value)
	{
		var ticks = Math.Clamp((long)value, DateTime.MinValue.Ticks, DateTime.MaxValue.Ticks);
		return new DateTime(ticks).ToString("yyyy MM");
	}
}
