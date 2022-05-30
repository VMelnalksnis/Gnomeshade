// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections;

using Gnomeshade.Interfaces.WebApi.V1_0.Transactions;

using NodaTime;

namespace Gnomeshade.Interfaces.WebApi.Tests.V1_0.Transactions;

public sealed class FromOptionalTestCaseSource : IEnumerable
{
	public IEnumerator GetEnumerator()
	{
		var now = SystemClock.Instance.GetCurrentInstant();

		yield return
			new TestCaseData(
					new OptionalTimeRange(),
					now,
					new TimeRange(Instant.FromUtc(now.InUtc().Year, now.InUtc().Month, 01, 00, 00, 00), now))
				.SetName("Defaults from start of current month to now");

		var from = now - Duration.FromDays(31);
		yield return
			new TestCaseData(
					new OptionalTimeRange { From = from },
					now,
					new TimeRange(from, now))
				.SetName("'from' defaults to the start of the 'to' date");

		from = now - Duration.FromDays(1);
		yield return
			new TestCaseData(
					new OptionalTimeRange { From = from, To = now },
					now,
					new TimeRange(from, now))
				.SetName("Returns same values if both given");
	}
}
