// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Markup.Xaml;

using Gnomeshade.Interfaces.Desktop.ViewModels;

namespace Gnomeshade.Interfaces.Desktop.Views;

/// <summary>
/// Split single transaction item into multiple.
/// </summary>
public sealed class TransactionItemSplitView : FocusOnInitUserControl, IView<TransactionItemSplitViewModel>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TransactionItemSplitView"/> class.
	/// </summary>
	public TransactionItemSplitView()
	{
		AvaloniaXamlLoader.Load(this);

		// Focus("SourceAccount");
	}
}
