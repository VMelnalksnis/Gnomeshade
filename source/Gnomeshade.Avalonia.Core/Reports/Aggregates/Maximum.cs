// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace Gnomeshade.Avalonia.Core.Reports.Aggregates;

/// <summary>Aggregates the collection to the largest value.</summary>
/// <seealso cref="Enumerable.Max(IEnumerable{decimal})"/>
public sealed class Maximum : IAggregateFunction
{
	/// <inheritdoc />
	public string Name => nameof(Maximum);

	decimal IAggregateFunction.Aggregate(IEnumerable<decimal> source) => source.Max();
}
