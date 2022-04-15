// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections;

using NodaTime;

using NUnit.Framework;

namespace Gnomeshade.Interfaces.WebApi.Client.Tests;

public sealed class TransactionUriTestCaseSource : IEnumerable
{
	public IEnumerator GetEnumerator()
	{
		var now = Instant.FromUtc(2021, 05, 21, 13, 05, 29);
		var firstDay = Instant.FromUtc(now.InUtc().Year, now.InUtc().Month, 1, 0, 0, 0);

		yield return
			new TestCaseData(
					null,
					null,
					"Transactions")
				.SetName("Without parameters");

		yield return
			new TestCaseData(
					firstDay,
					null,
					"Transactions?from=2021-05-01T00:00:00Z")
				.SetName("With only from date");

		yield return
			new TestCaseData(
					null,
					now,
					"Transactions?to=2021-05-21T13:05:29Z")
				.SetName("With only to date");

		yield return
			new TestCaseData(
					firstDay,
					now,
					"Transactions?from=2021-05-01T00:00:00Z&to=2021-05-21T13:05:29Z")
				.SetName("With both dates");
	}
}
