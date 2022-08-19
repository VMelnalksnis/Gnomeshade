// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections;
using System.Linq;
using System.Reflection;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.DesignTime;

using Moq;

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
			_viewLocator.Build(viewModel).Should().BeAssignableTo<IView<IControl, TViewModel>>();
			_viewLocator.Build(viewModel).Should().BeAssignableTo<IView<IControl, ViewModelBase>>();
		}
	}

	private static IEnumerable ViewTestCaseData()
	{
		AvaloniaLocator.CurrentMutable.Bind<ICursorFactory>().ToSingleton<CursorFactory>();

		return typeof(DesignTimeData)
			.GetProperties(BindingFlags.Public | BindingFlags.Static)
			.Where(property => property.PropertyType.IsAssignableTo(typeof(ViewModelBase)))
			.Where(property => property.PropertyType != typeof(MainWindowViewModel))
			.Select(property => new TestCaseData((ViewModelBase)property.GetValue(null)!).SetName(property.Name));
	}

	private class CursorFactory : ICursorFactory
	{
		public ICursorImpl GetCursor(StandardCursorType cursorType) => new Mock<ICursorImpl>().Object;

		public ICursorImpl CreateCursor(IBitmapImpl cursor, PixelPoint hotSpot) => new Mock<ICursorImpl>().Object;
	}
}
