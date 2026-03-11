// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;

using Gnomeshade.Avalonia.Core;

namespace Gnomeshade.Desktop.Views;

/// <inheritdoc cref="MainWindowViewModel"/>
public sealed partial class MainWindow : Window, IView<MainWindow, MainWindowViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="MainWindow"/> class.</summary>
	public MainWindow() => InitializeComponent();
}
