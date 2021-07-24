// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections;

using NUnit.Framework;

using Tracking.Finance.Interfaces.WebApi.V1_0.Transactions;

namespace Tracking.Finance.Interfaces.WebApi.Tests.V1_0.Transactions
{
	public sealed class FromOptionalTestCaseSource : IEnumerable
	{
		public IEnumerator GetEnumerator()
		{
			var now = DateTimeOffset.Now;

			yield return
				new TestCaseData(
						new OptionalTimeRange(),
						now,
						new TimeRange(new(now.Year, now.Month, 01, 00, 00, 00, now.Offset), now))
					.SetName("Defaults from start of current month to now");

			var from = now.AddMonths(-1);
			yield return
				new TestCaseData(
						new OptionalTimeRange { From = from },
						now,
						new TimeRange(from, now))
					.SetName("'from' defaults to the start of the 'to' date");

			from = now.AddDays(-1);
			yield return
				new TestCaseData(
						new OptionalTimeRange { From = from, To = now },
						now,
						new TimeRange(from, now))
					.SetName("Returns same values if both given");
		}
	}
}
