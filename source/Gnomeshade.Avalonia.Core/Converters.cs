// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections;

using Avalonia.Data.Converters;

namespace Gnomeshade.Avalonia.Core;

/// <summary>Provides a set of <see cref="IValueConverter"/>s for working with common types that can be represented as strings.</summary>
public static class Converters
{
	/// <summary>Gets a <see cref="string"/>/<see cref="NodaTime.LocalDate"/> converter.</summary>
	public static LocalDateConverter LocalDate { get; } = new();

	/// <summary>Gets a <see cref="string"/>/<see cref="NodaTime.LocalDateTime"/> converter.</summary>
	public static LocalDateTimeConverter LocalDateTime { get; } = new();

	/// <summary>Gets a <see cref="string"/>/<see cref="NodaTime.Period"/> converter.</summary>
	public static PeriodConverter Period { get; } = new();

	/// <summary>Gets a <see cref="ICollection"/> converter that checks whether the collection is not empty.</summary>
	public static IValueConverter NotEmpty { get; } =
		new FuncValueConverter<ICollection?, bool>(collection => collection?.Count > 0);

	/// <summary>Gets a <see cref="IEnumerable"/> converter that checks whether it is not empty.</summary>
	public static IValueConverter Any { get; } = new FuncValueConverter<IEnumerable?, bool>(enumerable =>
	{
		var enumerator = enumerable?.GetEnumerator();
		using var disposable = enumerator as IDisposable;
		return enumerator?.MoveNext() ?? false;
	});
}
