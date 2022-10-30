// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;

using Avalonia.Collections;

namespace Gnomeshade.Avalonia.Core.Transactions;

/// <summary>Compares <see cref="TransactionOverview"/> by a <see cref="DateTimeOffset"/> property.</summary>
/// <seealso cref="DataGridComparerSortDescription"/>
public sealed class TransactionOverviewComparer : IComparer, IComparer<TransactionOverview?>
{
	private readonly Func<TransactionOverview?, DateTimeOffset?> _selector;

	/// <summary>Initializes a new instance of the <see cref="TransactionOverviewComparer"/> class.</summary>
	/// <param name="selector">Function for selecting the value by which to compare overviews.</param>
	public TransactionOverviewComparer(Func<TransactionOverview?, DateTimeOffset?> selector)
	{
		_selector = selector;
	}

	/// <inheritdoc />
	public int Compare(object? x, object? y) => Compare(x as TransactionOverview, y as TransactionOverview);

	/// <inheritdoc />
	public int Compare(TransactionOverview? x, TransactionOverview? y)
	{
		var first = _selector(x);
		var second = _selector(y);

		return (first, second) switch
		{
			(null, null) => 0,
			(null, _) => -1,
			(_, null) => 1,
			_ => first.Value.CompareTo(second.Value),
		};
	}
}
