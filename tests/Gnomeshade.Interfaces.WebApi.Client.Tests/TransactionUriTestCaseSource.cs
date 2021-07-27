// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections;

using NUnit.Framework;

namespace Gnomeshade.Interfaces.WebApi.Client.Tests
{
	public sealed class TransactionUriTestCaseSource : IEnumerable
	{
		public IEnumerator GetEnumerator()
		{
			var now = new DateTimeOffset(2021, 05, 21, 13, 05, 29, TimeSpan.FromHours(3));
			var firstDay = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);

			yield return
				new TestCaseData(
						null,
						null,
						"Transaction")
					.SetName("Without parameters");

			yield return
				new TestCaseData(
						firstDay,
						null,
						"Transaction?from=2021-05-01T00:00:00.0000000+03:00")
					.SetName("With only from date");

			yield return
				new TestCaseData(
						null,
						now,
						"Transaction?to=2021-05-21T13:05:29.0000000+03:00")
					.SetName("With only to date");

			yield return
				new TestCaseData(
						firstDay,
						now,
						"Transaction?from=2021-05-01T00:00:00.0000000+03:00&to=2021-05-21T13:05:29.0000000+03:00")
					.SetName("With both dates");
		}
	}
}
