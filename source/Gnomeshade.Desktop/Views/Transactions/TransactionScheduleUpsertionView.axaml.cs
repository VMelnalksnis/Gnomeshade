// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Transactions;
using Gnomeshade.Avalonia.Core.Transactions.Purchases;

namespace Gnomeshade.Desktop.Views.Transactions;

/// <inheritdoc cref="PurchaseUpsertionViewModel"/>
public sealed partial class TransactionScheduleUpsertionView : UserControl, IView<TransactionScheduleUpsertionView, TransactionScheduleUpsertionViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="TransactionScheduleUpsertionView"/> class.</summary>
	public TransactionScheduleUpsertionView() => InitializeComponent();
}
