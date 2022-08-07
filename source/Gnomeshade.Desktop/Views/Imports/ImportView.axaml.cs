// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Linq;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Imports;

using JetBrains.Annotations;

namespace Gnomeshade.Desktop.Views.Imports;

/// <summary>
/// Data importing and overview of import result.
/// </summary>
public sealed class ImportView : UserControl, IView<ImportViewModel>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ImportView"/> class.
	/// </summary>
	public ImportView()
	{
		AvaloniaXamlLoader.Load(this);
	}

	[PublicAPI]
	private async void SelectFile_OnClick(object? sender, RoutedEventArgs args)
	{
		var dialog = new OpenFileDialog
		{
			AllowMultiple = false,
			Title = "Select report file",
		};

		var fileNames = await dialog.ShowAsync((Window)this.GetVisualRoot());
		var path = this.FindControl<TextBlock>("Path");
		path.Text = fileNames?.Single();
	}
}
