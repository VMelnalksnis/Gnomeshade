// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;

using FluentAssertions;

using NUnit.Framework;

namespace Gnomeshade.Interfaces.Avalonia.Core.Tests;

public class ViewLocatorTests
{
	[Test]
	public void Build_ShouldReturnExpected()
	{
		var viewLocator = new ViewLocator();
		var viewModel = new TestViewModel();

		var accountView = viewLocator.Build(viewModel);
		accountView.Should().BeOfType<TestView>();

		var secondInstanceOfAccountView = viewLocator.Build(viewModel);
		secondInstanceOfAccountView.Should().NotBe(accountView);
	}

	private sealed class TestViewModel : ViewModelBase
	{
	}

	private sealed class TestView : UserControl, IView<TestViewModel>
	{
	}
}
