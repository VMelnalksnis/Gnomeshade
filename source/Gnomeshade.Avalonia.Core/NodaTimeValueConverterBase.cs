// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Globalization;

using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Gnomeshade.Avalonia.Core;

/// <inheritdoc />
public abstract class NodaTimeValueConverterBase : IValueConverter
{
	/// <summary>Notification when the input could not be converted.</summary>
	protected static readonly BindingNotification InvalidCastNotification =
		new(new InvalidCastException(), BindingErrorType.Error);

	/// <inheritdoc />
	public abstract object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture);

	/// <inheritdoc />
	public abstract object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture);
}
