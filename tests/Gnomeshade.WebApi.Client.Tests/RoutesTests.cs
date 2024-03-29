﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using NodaTime;

namespace Gnomeshade.WebApi.Client.Tests;

public class RoutesTests
{
	[Test]
	public void AccountUri_ShouldFormatGuidWithoutSeparators()
	{
		Routes.Accounts.IdUri(Guid.Empty).Should().Be("v1.0/Accounts/00000000000000000000000000000000");
	}

	[TestCaseSource(typeof(TransactionUriTestCaseSource))]
	public void TransactionUri_ShouldReturnExpected(Interval interval, string expectedUri)
	{
		Routes.Transactions.DateRangeUri(interval).Should().Be(expectedUri);
	}
}
