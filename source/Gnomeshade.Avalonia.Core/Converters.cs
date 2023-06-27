// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

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

	/// <summary>Gets a <see cref="ICollection"/> converter that checks whether the collection is not empty.</summary>
	public static IValueConverter IsNotEmptyCollection { get; } =
		new FuncValueConverter<ICollection?, bool>(collection => collection?.Count > 0);
}
