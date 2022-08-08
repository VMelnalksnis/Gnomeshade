// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;

namespace Gnomeshade.Avalonia.Core.Reports.Aggregates;

/// <summary>A function that aggregates a collection of values to a single value.</summary>
public interface IAggregateFunction
{
	/// <summary>Gets the name of the aggregation.</summary>
	string Name { get; }

	internal decimal Aggregate(IEnumerable<decimal> source);
}
