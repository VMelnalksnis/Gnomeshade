// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Gnomeshade.Interfaces.Desktop.Views;

/// <summary>
/// User control for viewing and editing a single account.
/// </summary>
public sealed class AccountDetailView : UserControl
{
	/// <summary>
	/// Initializes a new instance of the <see cref="AccountDetailView"/> class.
	/// </summary>
	public AccountDetailView()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
