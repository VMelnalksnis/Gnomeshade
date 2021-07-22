// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using Dapper;

namespace Tracking.Finance.Data.Repositories.Extensions
{
	/// <summary>
	/// Extension methods for reducing Dapper boilerplate.
	/// </summary>
	public static class DapperExtensions
	{
		/// <summary>
		/// Execute a query with a one-to-many mapping asynchronously using Task.
		/// </summary>
		/// <param name="dbConnection">The connection to query on.</param>
		/// <param name="command">The command to execute.</param>
		/// <typeparam name="TOne">The first type in the record set.</typeparam>
		/// <typeparam name="TMany">The second type in the record set.</typeparam>
		/// <returns>A collection of <typeparamref name="TOne"/> and the mapped collections of <typeparamref name="TMany"/>.</returns>
		public static async Task<IEnumerable<IGrouping<TOne, OneToOne<TOne, TMany>>>> QueryOneToManyAsync<TOne, TMany>(
			this IDbConnection dbConnection,
			CommandDefinition command)
		{
			var entries =
				await dbConnection
					.QueryAsync<TOne, TMany, OneToOne<TOne, TMany>>(command, (one, many) => new(one, many))
					.ConfigureAwait(false);

			return entries.GroupBy(entry => entry.First);
		}

		/// <summary>
		/// Execute a query with a one-to-many mapping, where the many also have a one-to-one mapping, asynchronously using a Task.
		/// </summary>
		/// <param name="dbConnection">The connection to query on.</param>
		/// <param name="command">The command to execute.</param>
		/// <typeparam name="T1">The first type in the record set.</typeparam>
		/// <typeparam name="T2">The second type in the record set.</typeparam>
		/// <typeparam name="T3">The third type in the record set.</typeparam>
		/// <returns>A collection of <typeparamref name="T1"/> and the mapped collections of <typeparamref name="T2"/> and <typeparamref name="T3"/> pairs.</returns>
		public static async Task<IEnumerable<IGrouping<T1, OneToOne<T1, OneToOne<T2, T3>>>>>
			QueryOneToManyAsync<T1, T2, T3>(
				this IDbConnection dbConnection,
				CommandDefinition command)
		{
			var entries =
				await dbConnection
					.QueryAsync<T1, T2, T3, OneToOne<T1, OneToOne<T2, T3>>>(
						command,
						(one, many, manyOther) => new(one, new(many, manyOther)))
					.ConfigureAwait(false);

			return entries.GroupBy(entry => entry.First);
		}
	}
}
