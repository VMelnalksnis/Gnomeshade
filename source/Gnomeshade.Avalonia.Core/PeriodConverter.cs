// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Globalization;

using NodaTime;
using NodaTime.Text;

namespace Gnomeshade.Avalonia.Core;

/// <summary>Converts a binding value of type <see cref="Period"/>.</summary>
public sealed class PeriodConverter : NodaTimeValueClassConverter<Period>
{
	/// <inheritdoc />
	protected override Period TemplateValue { get; } =
		Period.FromYears(1) + Period.FromMonths(6) + Period.FromWeeks(2) + Period.FromDays(5);

	/// <inheritdoc />
	protected override PeriodPattern GetPattern(CultureInfo culture) =>
		PeriodPattern.NormalizingIso;
}
