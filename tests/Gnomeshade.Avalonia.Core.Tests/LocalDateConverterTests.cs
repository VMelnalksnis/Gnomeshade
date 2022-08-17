// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Globalization;

namespace Gnomeshade.Avalonia.Core.Tests;

public sealed class LocalDateConverterTests
{
	private readonly LocalDateConverter _converter = new();

	[TestCaseSource(typeof(ConvertTestCaseSource))]
	public void Convert_ShouldReturnExpected<TExpected>(
		object? value,
		Type targetType,
		object? parameter,
		CultureInfo culture,
		TExpected expected)
	{
		_converter
			.Convert(value, targetType, parameter, culture)
			.Should()
			.BeEquivalentTo(expected);
	}

	[TestCaseSource(typeof(ConvertBackTestCaseSource))]
	public void ConvertBack_ShouldReturnExpected<TExpected>(
		object? value,
		Type targetType,
		object? parameter,
		CultureInfo culture,
		TExpected expected)
	{
		_converter
			.ConvertBack(value, targetType, parameter, culture)
			.Should()
			.BeEquivalentTo(expected);
	}
}
