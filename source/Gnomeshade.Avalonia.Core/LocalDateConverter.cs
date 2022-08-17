// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Globalization;

using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;

using NodaTime;
using NodaTime.Text;

namespace Gnomeshade.Avalonia.Core;

/// <summary>Converts a binding value of type <see cref="LocalDate"/>.</summary>
public sealed class LocalDateConverter : IValueConverter
{
	private const string _formatPattern = "d";
	private static readonly BindingNotification _invalidCastNotification = new(new InvalidCastException(), BindingErrorType.Error);

	/// <inheritdoc />
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is null)
		{
			return BindingNotification.Null;
		}

		if (!targetType.IsAssignableTo(typeof(string)) || value is not LocalDate localDate)
		{
			return _invalidCastNotification;
		}

		return GetPattern(culture).Format(localDate);
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
			return _invalidCastNotification;
		}

		if (string.IsNullOrWhiteSpace(text) && isNullableT)
		{
			return AvaloniaProperty.UnsetValue;
		}

		if (targetType.IsAssignableTo(typeof(LocalDate)) || targetType.IsAssignableTo(typeof(LocalDate?)))
		{
			var parseResult = GetPattern(culture).Parse(text);
			if (parseResult.Success)
			{
				return parseResult.Value;
			}

			return new BindingNotification(new FormatException(), BindingErrorType.DataValidationError);
		}

		return _invalidCastNotification;
	}

	private static LocalDatePattern GetPattern(CultureInfo culture) => LocalDatePattern.Create(_formatPattern, culture);
}
