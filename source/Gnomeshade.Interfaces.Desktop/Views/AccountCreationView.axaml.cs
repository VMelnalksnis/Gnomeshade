// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Markup.Xaml;

using Gnomeshade.Interfaces.Avalonia.Core;
using Gnomeshade.Interfaces.Desktop.ViewModels;

namespace Gnomeshade.Interfaces.Desktop.Views;

/// <summary>
/// User control for creating a new account.
/// </summary>
public sealed class AccountCreationView : FocusOnInitUserControl, IView<AccountCreationViewModel>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="AccountCreationView"/> class.
	/// </summary>
	public AccountCreationView()
	{
		AvaloniaXamlLoader.Load(this);
		Focus("AccountName");
	}
}
