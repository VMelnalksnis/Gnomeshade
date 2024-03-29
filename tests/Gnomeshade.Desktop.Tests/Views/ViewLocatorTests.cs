﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections;
using System.Linq;
using System.Reflection;

using Avalonia.Controls;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.DesignTime;

namespace Gnomeshade.Desktop.Tests.Views;

public sealed class ViewLocatorTests
{
	private readonly ViewLocator<App> _viewLocator = new();

	[TestCaseSource(nameof(ViewTestCaseData))]
	public void Build_ShouldReturnExpectedView<TViewModel>(TViewModel viewModel)
		where TViewModel : ViewModelBase
	{
		using (new AssertionScope())
		{
			_viewLocator.Build(viewModel).Should().BeAssignableTo<IView<Control, TViewModel>>();
			_viewLocator.Build(viewModel).Should().BeAssignableTo<IView<Control, ViewModelBase>>();
		}
	}

	private static IEnumerable ViewTestCaseData()
	{
		return typeof(DesignTimeData)
			.GetProperties(BindingFlags.Public | BindingFlags.Static)
			.Where(property => property.PropertyType.IsAssignableTo(typeof(ViewModelBase)))
			.Where(property => property.PropertyType != typeof(MainWindowViewModel))
			.Select(property => new TestCaseData((ViewModelBase)property.GetValue(null)!).SetName(property.Name));
	}
}
