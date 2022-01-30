// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Markup.Xaml;

using Gnomeshade.Interfaces.Desktop.ViewModels;

namespace Gnomeshade.Interfaces.Desktop.Views;

public sealed class TransactionItemCreationView : FocusOnInitUserControl, IView<TransactionItemCreationViewModel>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TransactionItemCreationView"/> class.
	/// </summary>
	public TransactionItemCreationView()
	{
		AvaloniaXamlLoader.Load(this);
		Focus("SourceAccount");
	}
}
