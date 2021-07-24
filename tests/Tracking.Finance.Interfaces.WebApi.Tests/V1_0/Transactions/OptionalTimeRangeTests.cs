// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.ComponentModel.DataAnnotations;

using FluentAssertions;

using NUnit.Framework;

using Tracking.Finance.Interfaces.WebApi.V1_0.Transactions;

namespace Tracking.Finance.Interfaces.WebApi.Tests.V1_0.Transactions
{
	public class OptionalTimeRangeTests
	{
		[TestCaseSource(typeof(ValidateTestCaseSource))]
		public void Validate_ShouldReturnExpected(
			OptionalTimeRange optionalTimeRange,
			int expectedResultCount)
		{
			var validationContext = new ValidationContext(optionalTimeRange);

			optionalTimeRange
				.Validate(validationContext)
				.Should()
				.HaveCount(expectedResultCount);
		}
	}
}
