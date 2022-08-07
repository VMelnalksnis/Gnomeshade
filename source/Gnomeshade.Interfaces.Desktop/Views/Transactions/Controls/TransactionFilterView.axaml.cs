// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Transactions.Controls;

namespace Gnomeshade.Interfaces.Desktop.Views.Transactions.Controls;

/// <inheritdoc cref="TransactionFilter" />
public sealed class TransactionFilterView : UserControl, IView<TransactionFilter>
{
	/// <summary>Initializes a new instance of the <see cref="TransactionFilterView"/> class.</summary>
	public TransactionFilterView()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
