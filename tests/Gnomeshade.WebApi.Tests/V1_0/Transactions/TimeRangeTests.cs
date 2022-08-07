// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.WebApi.V1_0.Transactions;

using NodaTime;

namespace Gnomeshade.WebApi.Tests.V1_0.Transactions;

public class TimeRangeTests
{
	[TestCaseSource(typeof(FromOptionalTestCaseSource))]
	public void FromOptional_ShouldReturnExpected(
		OptionalTimeRange optionalTimeRange,
		Instant currentTime,
		TimeRange expectedTimeRange)
	{
		var timeRange = TimeRange.FromOptional(optionalTimeRange, currentTime);

		timeRange.Should().Be(expectedTimeRange);
	}

	[Test]
	public void StartCannotBeBeforeEnd()
	{
		var start = Instant.FromUtc(2021, 12, 30, 07, 51, 03);
		var end = Instant.FromUtc(2021, 05, 20, 13, 05, 21);

		FluentActions
			.Invoking(() => new TimeRange(start, end))
			.Should()
			.ThrowExactly<ArgumentException>()
			.Which.ParamName
			.Should()
			.Be("start");
	}

	[Test]
	public void Deconstruct_ShouldReturnSameValues()
	{
		var expectedStart = SystemClock.Instance.GetCurrentInstant();
		var expectedEnd = expectedStart + Duration.FromDays(1);
		var timeRange = new TimeRange(expectedStart, expectedEnd);

		var (start, end) = timeRange;

		using (new AssertionScope())
		{
			start.Should().Be(expectedStart);
			end.Should().Be(expectedEnd);
		}
	}
}
