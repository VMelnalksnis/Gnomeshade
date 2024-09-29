// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Globalization;

using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Gnomeshade.Desktop.Views.Transactions;

internal sealed class ProjectionConverter : IValueConverter
{
	/// <inheritdoc />
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value is true
		? Brushes.DimGray
		: Brushes.Transparent;

	/// <inheritdoc />
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}
