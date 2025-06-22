// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Transactions.Purchases;

namespace Gnomeshade.Desktop.Views.Transactions.Purchases;

/// <inheritdoc cref="PlannedPurchaseViewModel" />
public sealed partial class PlannedPurchaseView : UserControl, IView<PlannedPurchaseView, PlannedPurchaseViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="PlannedPurchaseView"/> class.</summary>
	public PlannedPurchaseView() => InitializeComponent();
}
