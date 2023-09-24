// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;

namespace Gnomeshade.Avalonia.Core.Tests;

[TestOf(typeof(ViewLocator<>))]
public sealed class ViewLocatorTests
{
	private readonly ViewLocator<TestView> _viewLocator = new();

	[Test]
	public void Build_ShouldReturnExpected()
	{
		var viewModel = new TestViewModel(new StubbedActivityService());

		var accountView = _viewLocator.Build(viewModel);
		accountView.Should().BeOfType<TestView>();

		var secondInstanceOfAccountView = _viewLocator.Build(viewModel);
		secondInstanceOfAccountView.Should().NotBe(accountView);
	}

	[Test]
	public void Match_ShouldMatchOnlyViewModels()
	{
		var viewModel = new TestViewModel(new StubbedActivityService());

		using var scope = new AssertionScope();
		_viewLocator.Match(viewModel).Should().BeTrue();
		_viewLocator.Match(_viewLocator).Should().BeFalse();
		_viewLocator.Match(null).Should().BeFalse();
	}

	private sealed class TestViewModel(IActivityService activityService) : ViewModelBase(activityService);

	private sealed class TestView : UserControl, IView<TestView, TestViewModel>;
}
