// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Globalization;

using Avalonia;
using Avalonia.Data;

using NodaTime;

namespace Gnomeshade.Avalonia.Core.Tests;

internal sealed class ConvertBackTestCaseSource : IEnumerable
{
	public IEnumerator GetEnumerator()
	{
		yield return new TestCaseData(
				"12/31/9999",
				typeof(LocalDate),
				null,
				CultureInfo.InvariantCulture,
				LocalDate.MaxIsoValue)
			.SetName("Valid local date pattern");

		yield return new TestCaseData(
				"31/12/9999",
				typeof(LocalDate),
				null,
				CultureInfo.CreateSpecificCulture("en-GB"),
				LocalDate.MaxIsoValue)
			.SetName("Valid local date pattern respecting culture");

		yield return new TestCaseData(
				"31/12/9999",
				typeof(LocalDate),
				null,
				CultureInfo.InvariantCulture,
				new BindingNotification(new FormatException(), BindingErrorType.DataValidationError))
			.SetName("Invalid local date pattern to data validation notification");

		yield return new TestCaseData(
				null,
				typeof(LocalDate?),
				null,
				CultureInfo.InvariantCulture,
				AvaloniaProperty.UnsetValue)
			.SetName("Null to unset value for nullable date");

		yield return new TestCaseData(
				null,
				typeof(LocalDate),
				null,
				CultureInfo.InvariantCulture,
				new BindingNotification(new InvalidCastException(), BindingErrorType.Error))
			.SetName("Null to error notification for non-nullable date");

		yield return new TestCaseData(
				DateTime.Now,
				typeof(LocalDate),
				null,
				CultureInfo.InvariantCulture,
				new BindingNotification(new InvalidCastException(), BindingErrorType.Error))
			.SetName("Wrong value type to error notification");
	}
}
