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
		_filter = new(new StubbedActivityService(), SystemClock.Instance, DateTimeZoneProviders.Tzdb);
	}

	[Test]
	public void Filter_ShouldBeFalseIfTransferSumIsZero()
	{
		var overview = new TransactionOverview(
			Guid.Empty,
			null,
			null,
			null,
			[
				new("EUR", false, 100, "Foo", "→", false, "Bar", "Bar", "EUR", 100),
				new("EUR", false, 100, "Foo", "←", false, "Bar", "Bar", "EUR", 100)
			],
			[],
			[]);

		_filter.Uncategorized = true;
		var filtered = _filter.Filter(overview);

		filtered.Should().BeFalse();
	}

	[Test]
	public void Filter_Loan()
	{
		var loanId = Guid.NewGuid();
		var overview = new TransactionOverview(Guid.Empty, null, null, null, [], [], [new() { LoanId = loanId }]);

		_filter.Filter(overview).Should().BeTrue("should display all rows without any filters");

		_filter.SelectedLoan = new() { Id = Guid.NewGuid() };
		_filter.Filter(overview).Should().BeFalse("another loan selected");

		_filter.InvertLoan = true;
		_filter.Filter(overview).Should().BeTrue("another loan selected and filter is inverted");

		_filter.SelectedLoan = new() { Id = loanId };
		_filter.Filter(overview).Should().BeFalse("the loan is selected, but filter is inverted");

		_filter.InvertLoan = false;
		_filter.Filter(overview).Should().BeTrue("the loan is selected");
	}
}
