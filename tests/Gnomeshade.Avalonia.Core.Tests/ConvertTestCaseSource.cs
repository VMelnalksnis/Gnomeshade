// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Globalization;

using Avalonia.Data;

using NodaTime;

namespace Gnomeshade.Avalonia.Core.Tests;

internal sealed class ConvertTestCaseSource : IEnumerable
{
	public IEnumerator GetEnumerator()
	{
		yield return new TestCaseData(
				null,
				typeof(string),
				null,
				CultureInfo.InvariantCulture,
				BindingNotification.Null)
			.SetName("null should return notification");

		yield return new TestCaseData(
				LocalDate.MaxIsoValue,
				typeof(decimal),
				null,
				CultureInfo.InvariantCulture,
				new BindingNotification(new InvalidCastException(), BindingErrorType.Error))
			.SetName("Trying to convert to wrong type should return notification");

		yield return new TestCaseData(
				DateTime.Now,
				typeof(string),
				null,
				CultureInfo.InvariantCulture,
				new BindingNotification(new InvalidCastException(), BindingErrorType.Error))
			.SetName("Trying to convert from wrong type should return notification");

		yield return new TestCaseData(
				LocalDate.MaxIsoValue,
				typeof(string),
				null,
				CultureInfo.InvariantCulture,
				"12/31/9999")
			.SetName($"Converting {nameof(LocalDate)} to {nameof(String)}");

		yield return new TestCaseData(
				LocalDate.MaxIsoValue,
				typeof(string),
				null,
				CultureInfo.CreateSpecificCulture("en-GB"),
				"31/12/9999")
			.SetName($"Converting {nameof(LocalDate)} to {nameof(String)} should respect culture");
	}
}
