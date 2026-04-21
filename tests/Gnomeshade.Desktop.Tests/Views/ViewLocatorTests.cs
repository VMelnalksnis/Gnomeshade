// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections;
using System.Linq;
using System.Reflection;

using Avalonia.Controls;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.DesignTime;
using Gnomeshade.Avalonia.Core.Help;

namespace Gnomeshade.Desktop.Tests.Views;

public sealed class ViewLocatorTests
{
	private readonly ViewLocator<App> _viewLocator = new();

	[TestCaseSource(nameof(ViewTestCaseData))]
	public void Build_ShouldReturnExpectedView<TViewModel>(TViewModel viewModel)
		where TViewModel : ViewModelBase
	{
		var view = _viewLocator.Build(viewModel);

		using var scope = new AssertionScope();
		view.Should().BeAssignableTo<IView<Control, TViewModel>>();
		view.Should().BeAssignableTo<IView<Control, ViewModelBase>>();
	}

	private static IEnumerable ViewTestCaseData()
	{
		return typeof(DesignTimeData)
			.GetProperties(BindingFlags.Public | BindingFlags.Static)
			.Where(property => property.PropertyType.IsAssignableTo(typeof(ViewModelBase)))
			.Where(property =>
				property.PropertyType != typeof(MainWindowViewModel) &&
				property.PropertyType != typeof(LicensesViewModel))
			.Select(property => new TestCaseData((ViewModelBase)property.GetValue(null)!).SetName(property.Name));
	}
}
