// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Transactions.Purchases;

namespace Gnomeshade.Desktop.Views.Transactions.Purchases;

/// <inheritdoc cref="PlannedPurchaseUpsertionViewModel"/>
public sealed partial class PlannedPurchaseUpsertionView : UserControl, IView<PlannedPurchaseUpsertionView, PlannedPurchaseUpsertionViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="PlannedPurchaseUpsertionView"/> class.</summary>
	public PlannedPurchaseUpsertionView() => InitializeComponent();
}
