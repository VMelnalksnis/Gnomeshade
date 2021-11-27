// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using FluentAssertions;

using NUnit.Framework;

namespace Gnomeshade.Core.Tests;

public class Sha512ValueTests
{
	[TestCase("Foo", "Foo", true)]
	[TestCase("Foo", "Bar", false)]
	public async Task GetHashCode_Should(object first, object second, bool expected)
	{
		var firstValue = await first.GetHashAsync().ConfigureAwait(false);
		var secondValue = await second.GetHashAsync().ConfigureAwait(false);

		var areEqual = firstValue.GetHashCode() == secondValue.GetHashCode();

		areEqual.Should().Be(expected);
	}

	[TestCase(63, TestName = "Not enough bytes")]
	[TestCase(65, TestName = "Too many bytes")]
	public void ShouldAllowOnlyExpectedLength(int invalidByteCount)
	{
		FluentActions
			.Invoking(() => new Sha512Value(new byte[invalidByteCount]))
			.Should()
			.ThrowExactly<ArgumentException>()
			.Which.ParamName
			.Should()
			.Be("bytes");
	}
}
