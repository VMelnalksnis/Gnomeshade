// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace Gnomeshade.Avalonia.Core.Reports.Aggregates;

/// <summary>Sums all values within the range.</summary>
public sealed class Sum : IAggregateFunction
{
	/// <inheritdoc />
	public string Name => nameof(Sum);

	decimal IAggregateFunction.Aggregate(IEnumerable<decimal> source) => source.Sum();
}
