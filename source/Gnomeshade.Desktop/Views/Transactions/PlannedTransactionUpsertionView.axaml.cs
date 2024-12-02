// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Transactions;

namespace Gnomeshade.Desktop.Views.Transactions;

/// <inheritdoc cref="PlannedTransactionUpsertionViewModel" />
public sealed partial class PlannedTransactionUpsertionView : UserControl, IView<PlannedTransactionUpsertionView, PlannedTransactionUpsertionViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="PlannedTransactionUpsertionView"/> class.</summary>
	public PlannedTransactionUpsertionView() => InitializeComponent();
}
