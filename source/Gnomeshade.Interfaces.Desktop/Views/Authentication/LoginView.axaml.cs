﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Markup.Xaml;

using Gnomeshade.Interfaces.Avalonia.Core;
using Gnomeshade.Interfaces.Avalonia.Core.Authentication;

namespace Gnomeshade.Interfaces.Desktop.Views.Authentication;

/// <summary>Authenticate the current user.</summary>
public sealed class LoginView : FocusOnInitUserControl, IView<LoginViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="LoginView"/> class.</summary>
	public LoginView()
	{
		AvaloniaXamlLoader.Load(this);
		Focus("Username");
	}
}