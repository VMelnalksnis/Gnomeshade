// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Gnomeshade.Avalonia.Core.Reports;

internal static class CollectionExtensions
{
	internal static TResult? MinOrDefault<TSource, TResult>(
		this IReadOnlyCollection<TSource> source,
		Func<TSource, TResult> selector,
		TResult? defaultResult = default)
	{
		return source.Any() ? source.Min(selector) : defaultResult;
	}

	internal static TResult? MaxOrDefault<TSource, TResult>(
		this IReadOnlyCollection<TSource> source,
		Func<TSource, TResult> selector,
		TResult? defaultResult = default)
	{
		return source.Any() ? source.Max(selector) : defaultResult;
	}
}
