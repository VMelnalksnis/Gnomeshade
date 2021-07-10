// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tracking.Finance.Interfaces.Desktop.ViewModels
{
	/// <summary>
	/// Collection of async LINQ extension methods.
	/// </summary>
	public static class AsyncLinqExtensions
	{
		/// <summary>
		/// Asynchronously projects each element of a sequence into a new form.
		/// </summary>
		/// <param name="source">A sequence of values to invoke a transform function on.</param>
		/// <param name="selector">An asynchronous transform function to apply to each element.</param>
		/// <typeparam name="TSource">The type of elements of <paramref name="source"/>.</typeparam>
		/// <typeparam name="TResult">The type of value returned by <paramref name="selector"/>.</typeparam>
		/// <returns>A task that represents the completion of all element projection.</returns>
		public static async Task<IEnumerable<TResult>> SelectAsync<TSource, TResult>(
			this IEnumerable<TSource> source,
			Func<TSource, Task<TResult>> selector)
		{
			var tasks = source.Select(selector);
			return await Task.WhenAll(tasks).ConfigureAwait(false);
		}
	}
}
