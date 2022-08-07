// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections;

using Avalonia.Collections;

namespace Gnomeshade.Avalonia.Core.Transactions;

/// <summary>Compares <see cref="TransactionOverview"/> by a <see cref="DateTimeOffset"/> property.</summary>
/// <seealso cref="DataGridComparerSortDesctiption"/>
public sealed class TransactionOverviewComparer : IComparer
{
	private readonly Func<TransactionOverview?, DateTimeOffset?> _selector;

	/// <summary>Initializes a new instance of the <see cref="TransactionOverviewComparer"/> class.</summary>
	/// <param name="selector">Function for selecting the value by which to compare overviews.</param>
	public TransactionOverviewComparer(Func<TransactionOverview?, DateTimeOffset?> selector)
	{
		_selector = selector;
	}

	/// <inheritdoc />
	public int Compare(object? x, object? y)
	{
		var first = _selector(x as TransactionOverview);
		var second = _selector(y as TransactionOverview);
		return first switch
		{
			null when second is null => 0,
			null => -1,
			_ when second is null => 1,
			_ => first.Value.CompareTo(second.Value),
		};
	}
}
