// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Globalization;

using FluentAssertions;

using Gnomeshade.Interfaces.Desktop.ViewModels.Binding;

using NUnit.Framework;

namespace Gnomeshade.Interfaces.Desktop.Tests.ViewModels.Binding
{
	public class DataGridTypedGroupDescriptionTests
	{
		private DataGridTypedGroupDescription<Foobar, string> _groupDescription = null!;

		[SetUp]
		public void SetUp()
		{
			_groupDescription = new(foobar => foobar.Foo);
		}

		[Test]
		public void PropertyName_ShouldReturnExpected()
		{
			_groupDescription.PropertyName.Should().Be(nameof(Foobar.Foo));
		}

		[TestCase("123", "123", true)]
		[TestCase("123", "456", false)]
		public void KeysMatch_ShouldReturnExpected(string fooGroup, string fooItem, bool match)
		{
			var group = new Foobar { Foo = fooGroup };
			var item = new Foobar { Foo = fooItem, Bar = "1" };

			var groupKey = _groupDescription.GroupKeyFromItem(group, default, CultureInfo.InvariantCulture);
			var itemKey = _groupDescription.GroupKeyFromItem(item, default, CultureInfo.InvariantCulture);

			_groupDescription.KeysMatch(groupKey, itemKey).Should().Be(match);
		}

		private class Foobar
		{
			public string Foo { get; init; } = string.Empty;

			// ReSharper disable once UnusedAutoPropertyAccessor.Local
			public string Bar { get; init; } = string.Empty;
		}
	}
}
