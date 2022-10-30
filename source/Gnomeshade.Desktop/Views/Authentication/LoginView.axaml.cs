// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Authentication;

namespace Gnomeshade.Desktop.Views.Authentication;

/// <inheritdoc cref="LoginViewModel"/>
public sealed partial class LoginView : UserControl, IView<LoginView, LoginViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="LoginView"/> class.</summary>
	public LoginView()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
