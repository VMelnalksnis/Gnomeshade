// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

#if DEBUG
using Avalonia;
#endif
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Gnomeshade.Desktop.Views;

/// <summary>The root element of a dialog window.</summary>
public sealed partial class DialogWindow : Window
{
	/// <summary>Initializes a new instance of the <see cref="DialogWindow"/> class.</summary>
	public DialogWindow()
	{
		AvaloniaXamlLoader.Load(this);

#if DEBUG
		this.AttachDevTools();
#endif
	}
}
