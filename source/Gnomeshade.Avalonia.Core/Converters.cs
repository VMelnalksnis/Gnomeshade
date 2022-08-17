// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Data.Converters;

namespace Gnomeshade.Avalonia.Core;

/// <summary>Provides a set of <see cref="IValueConverter"/>s for working with common types that can be represented as strings.</summary>
public static class Converters
{
	/// <summary>Gets a <see cref="string"/>/<see cref="System.DateTime"/> converter.</summary>
	public static DateTimeConverter DateTime { get; } = new();

	/// <summary>Gets a <see cref="string"/>/<see cref="NodaTime.LocalDate"/> converter.</summary>
	public static LocalDateConverter LocalDate { get; } = new();
}
