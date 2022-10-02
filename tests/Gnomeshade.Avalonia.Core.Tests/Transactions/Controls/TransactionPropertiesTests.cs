// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections;

using Gnomeshade.Avalonia.Core.Transactions.Controls;

namespace Gnomeshade.Avalonia.Core.Tests.Transactions.Controls;

[TestOf(typeof(TransactionProperties))]
public class TransactionPropertiesTests
{
	[TestCaseSource(nameof(IsValidTestCaseSource))]
	public void IsValid_ShouldBeExpected(TransactionProperties transactionProperties, bool expected)
	{
		transactionProperties.IsValid.Should().Be(expected);
	}

	private static IEnumerable IsValidTestCaseSource()
	{
		yield return new TestCaseData(
			new TransactionProperties(new ActivityService()),
			false);

		yield return new TestCaseData(
			new TransactionProperties(new ActivityService()) { BookingDate = DateTime.UtcNow, ValueTime = TimeSpan.FromSeconds(1) },
			false);

		yield return new TestCaseData(
			new TransactionProperties(new ActivityService()) { BookingDate = DateTime.UtcNow, BookingTime = TimeSpan.FromSeconds(1) },
			true);

		yield return new TestCaseData(
			new TransactionProperties(new ActivityService()) { ValueDate = DateTime.UtcNow, ValueTime = TimeSpan.FromSeconds(1) },
			true);
	}
}
