// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace Gnomeshade.Avalonia.Core.Reports.Aggregates;

/// <summary>Aggregates the collection to the smallest value.</summary>
/// <seealso cref="Enumerable.Min(IEnumerable{decimal})"/>
public sealed class Minimum : IAggregateFunction
{
	/// <inheritdoc />
	public string Name => nameof(Minimum);

	decimal IAggregateFunction.Aggregate(IEnumerable<decimal> source) => source.Min();
}
