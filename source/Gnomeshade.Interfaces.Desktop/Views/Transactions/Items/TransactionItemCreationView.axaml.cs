// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Markup.Xaml;

using Gnomeshade.Interfaces.Avalonia.Core;
using Gnomeshade.Interfaces.Avalonia.Core.Transactions.Items;

namespace Gnomeshade.Interfaces.Desktop.Views.Transactions.Items;

/// <summary>Create or update a single transaction item.</summary>
public sealed class TransactionItemCreationView : FocusOnInitUserControl, IView<TransactionItemCreationViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="TransactionItemCreationView"/> class.</summary>
	public TransactionItemCreationView()
	{
		AvaloniaXamlLoader.Load(this);
		Focus("SourceAccount");
	}
}
