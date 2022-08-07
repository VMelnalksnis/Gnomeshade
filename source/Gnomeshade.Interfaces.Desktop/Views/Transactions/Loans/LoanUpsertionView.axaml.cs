// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Transactions.Loans;

namespace Gnomeshade.Interfaces.Desktop.Views.Transactions.Loans;

/// <inheritdoc cref="LoanUpsertionViewModel"/>
public sealed class LoanUpsertionView : UserControl, IView<LoanUpsertionViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="LoanUpsertionView"/> class.</summary>
	public LoanUpsertionView()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
