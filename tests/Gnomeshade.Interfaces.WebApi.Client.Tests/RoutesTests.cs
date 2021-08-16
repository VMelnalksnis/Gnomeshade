// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using FluentAssertions;

using NUnit.Framework;

namespace Gnomeshade.Interfaces.WebApi.Client.Tests
{
	public class RoutesTests
	{
		[Test]
		public void AccountUri_ShouldFormatGuidWithoutSeparators()
		{
			Routes.AccountIdUri(Guid.Empty).Should().Be("Account/00000000000000000000000000000000");
		}

		[TestCaseSource(typeof(TransactionUriTestCaseSource))]
		public void TransactionUri_ShouldReturnExpected(DateTimeOffset? from, DateTimeOffset? to, string expectedUri)
		{
			Routes.TransactionDateRangeUri(from, to).Should().Be(expectedUri);
		}

		[TestCase(3, TestName = "Positive offset contains '+'")]
		[TestCase(0, TestName = "Zero offset contains '+'")]
		public void UrlEncodeDateTimeOffset_ShouldNotContainPlus(int offsetHours)
		{
			var date = new DateTimeOffset(2021, 05, 30, 19, 54, 00, TimeSpan.FromHours(offsetHours));
			var encoded = Routes.UrlEncodeDateTimeOffset(date);
			encoded.Should().Be($"2021-05-30T19:54:00.0000000%2B{offsetHours:00}:00");
		}
	}
}
