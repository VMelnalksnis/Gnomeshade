using System.Collections.Generic;
using System.Linq;

using AutoMapper;

namespace Tracking.Finance.Interfaces.WebApi.v1_0.Transactions
{
	public static class MapperExtensions
	{
		public static IEnumerable<TDestination> MapCollection<TSource, TDestination>(this Mapper mapper, IEnumerable<TSource> sources)
		{
			return sources.Select(source => mapper.Map<TSource, TDestination>(source));
		}
	}
}
