// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using FluentAssertions;
using FluentAssertions.Execution;

using NUnit.Framework;

using Tracking.Finance.Interfaces.WebApi.V1_0.Transactions;

namespace Tracking.Finance.Interfaces.WebApi.Tests.V1_0.Transactions
{
	public class TimeRangeTests
	{
		[TestCaseSource(typeof(FromOptionalTestCaseSource))]
		public void FromOptional_ShouldReturnExpected(
			OptionalTimeRange optionalTimeRange,
			DateTimeOffset currentTime,
			TimeRange expectedTimeRange)
		{
			var timeRange = TimeRange.FromOptional(optionalTimeRange, currentTime);

			timeRange.Should().Be(expectedTimeRange);
		}

		[Test]
		public void StartCannotBeBeforeEnd()
		{
			var start = new DateTimeOffset(2021, 12, 30, 07, 51, 03, TimeSpan.Zero);
			var end = new DateTimeOffset(2021, 05, 20, 13, 05, 21, TimeSpan.Zero);

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
			var expectedStart = DateTimeOffset.Now;
			var expectedEnd = expectedStart.AddDays(1);
			var timeRange = new TimeRange(expectedStart, expectedEnd);

			var (start, end) = timeRange;

			using (new AssertionScope())
			{
				start.Should().Be(expectedStart);
				end.Should().Be(expectedEnd);
			}
		}
	}
}
