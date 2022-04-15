// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using FluentAssertions;

using NodaTime;

using NUnit.Framework;

namespace Gnomeshade.Interfaces.WebApi.Client.Tests;

public class RoutesTests
{
	[Test]
	public void AccountUri_ShouldFormatGuidWithoutSeparators()
	{
		Routes.Accounts.IdUri(Guid.Empty).Should().Be("Accounts/00000000000000000000000000000000");
	}

	[TestCaseSource(typeof(TransactionUriTestCaseSource))]
	public void TransactionUri_ShouldReturnExpected(Instant? from, Instant? to, string expectedUri)
	{
		Routes.Transactions.DateRangeUri(from, to).Should().Be(expectedUri);
	}
}
