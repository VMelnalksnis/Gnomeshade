﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Globalization;

using NodaTime;
using NodaTime.Text;

namespace Gnomeshade.Avalonia.Core;

/// <summary>Converts a binding value of type <see cref="LocalDate"/>.</summary>
public sealed class LocalDateConverter : NodaTimeValueConverter<LocalDate>
{
	/// <inheritdoc />
	public override string PatternText => GetPattern(CultureInfo.CurrentCulture).PatternText;

	/// <inheritdoc />
	protected override LocalDatePattern GetPattern(CultureInfo culture) =>
		LocalDatePattern.Create("d", culture);
}
