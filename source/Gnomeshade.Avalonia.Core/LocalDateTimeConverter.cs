// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Globalization;

using NodaTime;
using NodaTime.Text;

namespace Gnomeshade.Avalonia.Core;

/// <summary>Converts a binding value of type <see cref="LocalDateTime"/>.</summary>
public sealed class LocalDateTimeConverter : NodaTimeValueConverter<LocalDateTime>
{
	/// <inheritdoc />
	protected override LocalDateTime TemplateValue { get; } = new(2000, 12, 31, 13, 20);

	/// <inheritdoc />
	protected override LocalDateTimePattern GetPattern(CultureInfo culture) =>
		LocalDateTimePattern.Create("g", culture);
}
