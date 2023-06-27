// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Globalization;

using Avalonia;
using Avalonia.Data;

using NodaTime.Text;

namespace Gnomeshade.Avalonia.Core;

/// <inheritdoc />
public abstract class NodaTimeValueConverter<TTime> : NodaTimeValueConverterBase
	where TTime : struct
{
	/// <summary>Gets the string format that will be used to convert <typeparamref name="TTime"/> to and from text.</summary>
	public abstract string PatternText { get; }

	/// <inheritdoc />
	public override object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is null)
		{
			return BindingNotification.Null;
		}

		if (!targetType.IsAssignableTo(typeof(string)) || value is not TTime time)
		{
			return InvalidCastNotification;
		}

		return GetPattern(culture).Format(time);
	}

	/// <inheritdoc />
	public override object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		var isNullableT = targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>);
		if (value is null && isNullableT)
		{
			return AvaloniaProperty.UnsetValue;
		}

		if (value is not string text)
		{
			return InvalidCastNotification;
		}

		if (string.IsNullOrWhiteSpace(text) && isNullableT)
		{
			return AvaloniaProperty.UnsetValue;
		}

		if (targetType.IsAssignableTo(typeof(TTime)) || targetType.IsAssignableTo(typeof(TTime?)))
		{
			var parseResult = GetPattern(culture).Parse(text);
			if (parseResult.Success)
			{
				return parseResult.Value;
			}

			return new BindingNotification(new FormatException(), BindingErrorType.DataValidationError);
		}

		return InvalidCastNotification;
	}

	/// <summary>Gets the pattern using which <typeparamref name="TTime"/> will be convert to and from text.</summary>
	/// <param name="culture">The culture to use.</param>
	/// <returns>A pattern that can convert <typeparamref name="TTime"/> to and form text.</returns>
	protected abstract IPattern<TTime> GetPattern(CultureInfo culture);
}
