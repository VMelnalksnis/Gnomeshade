// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Transactions.Purchases;

namespace Gnomeshade.Desktop.Views.Transactions.Purchases;

/// <inheritdoc cref="PurchaseUpsertionViewModel"/>
public sealed class PurchaseUpsertionView : UserControl, IView<PurchaseUpsertionView, PurchaseUpsertionViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="PurchaseUpsertionView"/> class.</summary>
	public PurchaseUpsertionView()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
