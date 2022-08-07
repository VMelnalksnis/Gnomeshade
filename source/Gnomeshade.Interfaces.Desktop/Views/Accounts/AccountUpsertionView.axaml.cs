// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Accounts;

namespace Gnomeshade.Interfaces.Desktop.Views.Accounts;

/// <inheritdoc cref="AccountUpsertionViewModel"/>
public sealed class AccountUpsertionView : UserControl, IView<AccountUpsertionViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="AccountUpsertionView"/> class.</summary>
	public AccountUpsertionView()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
