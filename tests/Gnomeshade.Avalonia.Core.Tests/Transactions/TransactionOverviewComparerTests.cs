// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Avalonia.Core.Transactions;

namespace Gnomeshade.Avalonia.Core.Tests.Transactions;

[TestOf(typeof(TransactionOverviewComparer))]
public sealed class TransactionOverviewComparerTests
{
	[Test]
	public void Compare_ShouldReturnExpected()
	{
		var comparer = new TransactionOverviewComparer(overview => overview?.Date);

		var firstDate = new DateTimeOffset(new(2023, 05, 01));
		var secondDate = new DateTimeOffset(new(2023, 05, 02));

		var result = comparer.Compare(GetOverviewWithDate(firstDate), GetOverviewWithDate(secondDate));

		using var scope = new AssertionScope();
		result.Should().BeNegative();
		result.Should().Be(firstDate.CompareTo(secondDate));
	}

	private static TransactionOverview GetOverviewWithDate(DateTimeOffset date) =>
		new(Guid.NewGuid(), date, null, null, new(), new());
}
