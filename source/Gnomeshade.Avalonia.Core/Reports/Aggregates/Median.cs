// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;

namespace Gnomeshade.Avalonia.Core.Reports.Aggregates;

/// <summary>Aggregates the collection to the median value.</summary>
public sealed class Median : IAggregateFunction
{
	/// <inheritdoc />
	public string Name => nameof(Median);

	decimal IAggregateFunction.Aggregate(IEnumerable<decimal> source)
	{
		var sorted = source.ToImmutableSortedSet();
		var midPoint = sorted.Count / 2;

		return sorted.Count switch
		{
			0 => 0,
			1 => sorted[0],
			_ when sorted.Count % 2 is 1 => sorted[midPoint + 1],
			_ => (sorted[midPoint] + sorted[midPoint - 1]) / 2,
		};
	}
}
