// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Accounts;

namespace Gnomeshade.Interfaces.Desktop.Views.Accounts;

/// <summary>
/// An overview of of all accounts.
/// </summary>
public sealed class AccountView : UserControl, IView<AccountViewModel>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="AccountView"/> class.
	/// </summary>
	public AccountView()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
