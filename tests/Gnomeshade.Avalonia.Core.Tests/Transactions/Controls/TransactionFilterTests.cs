// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Avalonia.Core.Transactions;
using Gnomeshade.Avalonia.Core.Transactions.Controls;

using NodaTime;

namespace Gnomeshade.Avalonia.Core.Tests.Transactions.Controls;

[TestOf(typeof(TransactionFilter))]
public sealed class TransactionFilterTests
{
	private readonly TransactionFilter _filter;

	public TransactionFilterTests()
	{
		_filter = new(new ActivityService(), SystemClock.Instance, DateTimeZoneProviders.Tzdb);
	}

	[Test]
	public void Filter_ShouldBeFalseIfTransferSumIsZero()
	{
		var overview = new TransactionOverview(
			Guid.Empty,
			null,
			null,
			null,
			new()
			{
				new("EUR", false, 100, "Foo", "→", false, "Bar", "Bar", "EUR", 100),
				new("EUR", false, 100, "Foo", "←", false, "Bar", "Bar", "EUR", 100),
			},
			new(),
			false);

		_filter.Uncategorized = true;
		var filtered = _filter.Filter(overview);

		filtered.Should().BeFalse();
	}
}
