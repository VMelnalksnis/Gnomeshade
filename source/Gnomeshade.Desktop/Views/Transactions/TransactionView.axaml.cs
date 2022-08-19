// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Transactions;

namespace Gnomeshade.Desktop.Views.Transactions;

/// <inheritdoc cref="TransactionViewModel" />
public sealed class TransactionView : UserControl, IView<TransactionView, TransactionViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="TransactionView"/> class.</summary>
	public TransactionView()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
