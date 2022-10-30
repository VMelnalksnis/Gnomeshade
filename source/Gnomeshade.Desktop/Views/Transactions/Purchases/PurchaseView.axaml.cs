// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Transactions.Purchases;

namespace Gnomeshade.Desktop.Views.Transactions.Purchases;

/// <inheritdoc cref="PurchaseViewModel" />
public sealed partial class PurchaseView : UserControl, IView<PurchaseView, PurchaseViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="PurchaseView"/> class.</summary>
	public PurchaseView()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
