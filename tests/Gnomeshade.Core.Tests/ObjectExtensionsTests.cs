// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Threading.Tasks;

using JetBrains.Annotations;

namespace Gnomeshade.Core.Tests;

public class ObjectExtensionsTests
{
	[Test]
	public void GetHash_ShouldBeEqualForEquivalent()
	{
		var firstHolder = new AccountHolder { Name = "Foobar", LegalId = 12345678987 };
		var secondHolder = firstHolder with { };

		ReferenceEquals(firstHolder, secondHolder).Should().BeFalse();
		firstHolder.GetHash().Should().BeEquivalentTo(secondHolder.GetHash());
	}

	[Test]
	public void GetHash_ShouldBeDifferentForDifferentValues()
	{
		var firstValue = new AccountHolder { Name = "Foo", LegalId = 12345678987 }.GetHash();
		var secondValue = new AccountHolder { Name = "Bar", LegalId = 98765432123 }.GetHash();

		using (new AssertionScope())
		{
			firstValue.Should().NotBeEquivalentTo(secondValue);
			(firstValue != secondValue).Should().BeTrue();
		}
	}

	[Test]
	public async Task GetHashAsync_ShouldBeSameAsSync()
	{
		var holder = new AccountHolder { Name = "Foobar", LegalId = 12345678987 };

		// ReSharper disable once MethodHasAsyncOverload
		var syncHash = holder.GetHash();
		var asyncHash = await holder.GetHashAsync();

		using (new AssertionScope())
		{
			asyncHash.Should().BeEquivalentTo(syncHash);
			(asyncHash == syncHash).Should().BeTrue();
		}
	}

	[UsedImplicitly(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.Members)]
	public sealed record AccountHolder
	{
		public string Name { get; init; } = string.Empty;

		public long LegalId { get; init; }
	}
}
