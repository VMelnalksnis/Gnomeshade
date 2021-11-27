// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Gnomeshade.Interfaces.Desktop.Views;

public sealed class MainWindow : Window
{
	/// <summary>
	/// Initializes a new instance of the <see cref="MainWindow"/> class.
	/// </summary>
	public MainWindow()
	{
		AvaloniaXamlLoader.Load(this);
#if DEBUG
		this.AttachDevTools();
#endif
	}
}
