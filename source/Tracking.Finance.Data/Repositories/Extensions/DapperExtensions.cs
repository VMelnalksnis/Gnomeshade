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
		public static async Task<IEnumerable<OneToMany<TOne, TMany>>> QueryOneToManyAsync<TOne, TMany>(
			this IDbConnection dbConnection,
			CommandDefinition command)
		{
			var entries =
				await dbConnection
					.QueryAsync<TOne, TMany, OneToManyEntry<TOne, TMany>>(command, (one, many) => new(one, many))
					.ConfigureAwait(false);

			return entries
				.GroupBy(entry => entry.Parent)
				.Select(OneToMany<TOne, TMany>.FromEntryGrouping);
		}

		public static async Task<IEnumerable<OneToMany<T1, OneToOne<T2, T3>>>> QueryOneToManyAsync<T1, T2, T3>(
			this IDbConnection dbConnection,
			CommandDefinition command)
		{
			var entries =
				await dbConnection
					.QueryAsync<T1, T2, T3, OneToManyEntry<T1, OneToOne<T2, T3>>>(
						command,
						(one, many, manyOther) => new(one, new(many, manyOther)))
					.ConfigureAwait(false);

			return
				entries
					.GroupBy(entry => entry.Parent)
					.Select(OneToMany<T1, OneToOne<T2, T3>>.FromEntryGrouping);
		}
	}
}
