﻿using System;

using FluentAssertions;

using NUnit.Framework;

using Tracking.Finance.Interfaces.WebApi.v1_0.Authentication;
using Tracking.Finance.Interfaces.WebApi.v1_0.Transactions;

namespace Tracking.Finance.Interfaces.WebApi.Client.Tests
{
	public class TypeExtensionsTests
	{
		[Test]
		public void GetControllerName_ShouldThrowIfNotController()
		{
			FluentActions
				.Invoking(() => typeof(TypeExtensionsTests).GetControllerName())
				.Should()
				.ThrowExactly<ArgumentException>();
		}

		[TestCase(typeof(AuthenticationController), "Authentication")]
		[TestCase(typeof(TransactionController), "Transaction")]
		public void GetControllerName_ShouldReturnExpected(Type type, string expectedName)
		{
			type.GetControllerName().Should().Be(expectedName);
		}
	}
}
