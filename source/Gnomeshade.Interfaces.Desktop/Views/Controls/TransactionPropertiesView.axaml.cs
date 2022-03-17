// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Gnomeshade.Interfaces.Avalonia.Core;
using Gnomeshade.Interfaces.Avalonia.Core.Transactions.Controls;

namespace Gnomeshade.Interfaces.Desktop.Views.Controls;

/// <summary>Editor for transaction information besides transaction items.</summary>
public sealed class TransactionPropertiesView : UserControl, IView<TransactionProperties>
{
	/// <summary>Initializes a new instance of the <see cref="TransactionPropertiesView"/> class.</summary>
	public TransactionPropertiesView()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
