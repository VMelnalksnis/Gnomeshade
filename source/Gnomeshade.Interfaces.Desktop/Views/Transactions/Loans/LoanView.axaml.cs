// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Transactions.Loans;

namespace Gnomeshade.Interfaces.Desktop.Views.Transactions.Loans;

/// <inheritdoc cref="LoanViewModel" />
public sealed class LoanView : UserControl, IView<LoanViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="LoanView"/> class.</summary>
	public LoanView()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
