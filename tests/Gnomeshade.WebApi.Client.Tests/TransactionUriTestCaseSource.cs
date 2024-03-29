﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections;

using NodaTime;

namespace Gnomeshade.WebApi.Client.Tests;

public sealed class TransactionUriTestCaseSource : IEnumerable
{
	public IEnumerator GetEnumerator()
	{
		var now = Instant.FromUtc(2021, 05, 21, 13, 05, 29);
		var firstDay = Instant.FromUtc(now.InUtc().Year, now.InUtc().Month, 1, 0, 0, 0);

		yield return new TestCaseData(
				new Interval(null, null),
				"v1.0/Transactions")
			.SetName("Without parameters");

		yield return new TestCaseData(
				new Interval(firstDay, null),
				"v1.0/Transactions?from=2021-05-01T00:00:00Z")
			.SetName("With only from date");

		yield return new TestCaseData(
				new Interval(null, now),
				"v1.0/Transactions?to=2021-05-21T13:05:29Z")
			.SetName("With only to date");

		yield return new TestCaseData(
				new Interval(firstDay, now),
				"v1.0/Transactions?from=2021-05-01T00:00:00Z&to=2021-05-21T13:05:29Z")
			.SetName("With both dates");
	}
}
