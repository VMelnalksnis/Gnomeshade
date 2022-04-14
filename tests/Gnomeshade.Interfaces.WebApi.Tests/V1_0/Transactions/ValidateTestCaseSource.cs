// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections;

using Gnomeshade.Interfaces.WebApi.V1_0.Transactions;

using NodaTime;

using NUnit.Framework;

namespace Gnomeshade.Interfaces.WebApi.Tests.V1_0.Transactions;

public sealed class ValidateTestCaseSource : IEnumerable
{
	public IEnumerator GetEnumerator()
	{
		var timeRange = new OptionalTimeRange();
		yield return new TestCaseData(timeRange, 0)
			.SetName("Time range with no values is valid");

		timeRange = new() { From = SystemClock.Instance.GetCurrentInstant() };
		yield return new TestCaseData(timeRange, 0)
			.SetName($"Time range with only {nameof(OptionalTimeRange.From)} is valid");

		timeRange = new() { To = SystemClock.Instance.GetCurrentInstant() };
		yield return new TestCaseData(timeRange, 0)
			.SetName($"Time range with only {nameof(OptionalTimeRange.To)} is valid");

		timeRange = new()
		{
			From = SystemClock.Instance.GetCurrentInstant() - Duration.FromDays(1),
			To = SystemClock.Instance.GetCurrentInstant(),
		};
		yield return new TestCaseData(timeRange, 0)
			.SetName($"Time range where {nameof(OptionalTimeRange.From)} is less than {nameof(OptionalTimeRange.To)} is valid");

		timeRange = new()
		{
			From = SystemClock.Instance.GetCurrentInstant() + Duration.FromDays(1),
			To = SystemClock.Instance.GetCurrentInstant(),
		};
		yield return new TestCaseData(timeRange, 1)
			.SetName($"Time range where {nameof(OptionalTimeRange.From)} is greater than {nameof(OptionalTimeRange.To)} is not valid");
	}
}
