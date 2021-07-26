// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq.Expressions;

using FluentAssertions.Equivalency;

namespace Gnomeshade.Interfaces.WebApi.Tests.V1_0
{
	public static class FluentAssertionsExtensions
	{
		public static EquivalencyAssertionOptions<TExpectation> ByMembers<TExpectation, TActual>(
			this EquivalencyAssertionOptions<TExpectation> options)
		{
			return options.ComparingByMembers<TExpectation>().ComparingByMembers<TActual>();
		}

		public static EquivalencyAssertionOptions<TExpectation> ByMembersExcluding<TExpectation, TActual>(
			this EquivalencyAssertionOptions<TExpectation> options,
			Expression<Func<TExpectation, object?>> excluding)
		{
			return options.ComparingByMembers<TExpectation>().ComparingByMembers<TActual>().Excluding(excluding);
		}

		public static EquivalencyAssertionOptions<TExpectation> ByMembersExcluding<TExpectation, TActual>(
			this EquivalencyAssertionOptions<TExpectation> options,
			params Expression<Func<TExpectation, object?>>[] excluding)
		{
			options.ComparingByMembers<TExpectation>().ComparingByMembers<TActual>();
			foreach (var expression in excluding)
			{
				options.Excluding(expression);
			}

			return options;
		}
	}
}
