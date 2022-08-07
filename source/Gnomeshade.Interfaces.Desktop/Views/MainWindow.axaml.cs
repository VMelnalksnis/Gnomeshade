// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Diagnostics;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Gnomeshade.Avalonia.Core;

namespace Gnomeshade.Interfaces.Desktop.Views;

/// <summary>The root element of the application.</summary>
public sealed class MainWindow : Window, IView<MainWindowViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="MainWindow"/> class.</summary>
	public MainWindow()
	{
		AvaloniaXamlLoader.Load(this);
		if (Debugger.IsAttached)
		{
			this.AttachDevTools();
		}
	}
}
