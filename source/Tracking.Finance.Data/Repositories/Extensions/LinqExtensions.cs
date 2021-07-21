// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace Tracking.Finance.Data.Repositories.Extensions
{
	public static class LinqExtensions
	{
		public static T? SingleOrDefaultStruct<T>(this IEnumerable<T> source)
			where T : struct
		{
			return source.Select(element => (T?)element).SingleOrDefault();
		}
	}
}
