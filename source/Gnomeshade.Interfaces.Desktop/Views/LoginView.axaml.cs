// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Gnomeshade.Interfaces.Desktop.ViewModels;

namespace Gnomeshade.Interfaces.Desktop.Views;

public sealed class LoginView : UserControl, IView<LoginViewModel>
{
	private readonly TextBox _username;

	/// <summary>
	/// Initializes a new instance of the <see cref="LoginView"/> class.
	/// </summary>
	public LoginView()
	{
		AvaloniaXamlLoader.Load(this);

		_username = this.FindControl<TextBox>("Username");
		_username.AttachedToVisualTree += OnUsernameAttachedToVisualTree;
	}

	private void OnUsernameAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs eventArgs)
	{
		_username.Focus();
	}
}
