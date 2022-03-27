// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Gnomeshade.Interfaces.Avalonia.Core;
using Gnomeshade.Interfaces.Avalonia.Core.Transactions.Items;

namespace Gnomeshade.Interfaces.Desktop.Views.Transactions.Items;

/// <summary>
/// A detailed view of a single transaction.
/// </summary>
public sealed class TransactionDetailView : UserControl, IView<TransactionDetailViewModel>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TransactionDetailView"/> class.
	/// </summary>
	public TransactionDetailView()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
