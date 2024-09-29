// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

namespace Gnomeshade.Avalonia.Core.Reports.Splits;

internal static class SplitProvider
{
	internal static readonly MonthlySplit MonthlySplit = new();
	internal static readonly DailySplit DailySplit = new();

	internal static readonly IReportSplit[] Splits =
	[
		new YearlySplit(),
		MonthlySplit,
		new WeeklySplit(),
		DailySplit,
	];
}
