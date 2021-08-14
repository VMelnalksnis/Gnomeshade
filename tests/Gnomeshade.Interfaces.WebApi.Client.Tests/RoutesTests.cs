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

		[Test]
		public void UrlEncodeDateTimeOffset_ShouldNotContainPlus()
		{
			var date = new DateTimeOffset(2021, 05, 30, 19, 54, 00, TimeSpan.FromHours(3));
			var encoded = Routes.UrlEncodeDateTimeOffset(date);
			encoded.Should().Be("2021-05-30T19:54:00.0000000%2B03:00");
		}
	}
}
