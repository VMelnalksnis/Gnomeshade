// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Transactions.Transfers;

namespace Gnomeshade.Desktop.Views.Transactions.Transfers;

/// <inheritdoc cref="PlannedTransferViewModel" />
public sealed partial class PlannedTransferView : UserControl, IView<PlannedTransferView, PlannedTransferViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="PlannedTransferView"/> class.</summary>
	public PlannedTransferView() => InitializeComponent();
}
