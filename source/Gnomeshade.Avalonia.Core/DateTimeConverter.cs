// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Globalization;

using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Gnomeshade.Avalonia.Core;

/// <inheritdoc />
public sealed class DateTimeConverter : IValueConverter
{
	/// <inheritdoc />
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is null)
		{
			return BindingNotification.Null;
		}

		if (!targetType.IsAssignableTo(typeof(string)))
		{
			return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
		}

		return value switch
		{
			DateTimeOffset date => date.Date.ToString(culture.DateTimeFormat.ShortDatePattern, culture),
			TimeSpan time => time.ToString(GetTimeFormat(culture), culture),
			_ => new BindingNotification(new InvalidCastException(), BindingErrorType.Error),
		};
	}

	/// <inheritdoc />
	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		var isNullableT = targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>);
		if (value is null && isNullableT)
		{
			return AvaloniaProperty.UnsetValue;
		}

		if (value is not string text)
		{
			return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
		}

		if (string.IsNullOrWhiteSpace(text) && isNullableT)
		{
			return AvaloniaProperty.UnsetValue;
		}

		if (targetType.IsAssignableTo(typeof(DateTimeOffset)) || targetType.IsAssignableTo(typeof(DateTimeOffset?)))
		{
			if (DateTimeOffset.TryParseExact(text, culture.DateTimeFormat.ShortDatePattern, culture, DateTimeStyles.AssumeLocal, out var date))
			{
				return date;
			}

			return new BindingNotification(new FormatException(), BindingErrorType.DataValidationError);
		}

		if (targetType.IsAssignableTo(typeof(TimeSpan)) || targetType.IsAssignableTo(typeof(TimeSpan?)))
		{
			if (TimeSpan.TryParseExact(text, GetTimeFormat(culture), culture, out var time))
			{
				return time;
			}

			return new BindingNotification(new FormatException(), BindingErrorType.DataValidationError);
		}

		return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
	}

	private static string GetTimeFormat(CultureInfo culture) => $"hh\\{culture.DateTimeFormat.TimeSeparator}mm";
}
