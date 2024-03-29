// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Loans;

namespace Gnomeshade.Desktop.Views.Loans;

/// <inheritdoc cref="LoanViewModel" />
public sealed partial class LoanView : UserControl, IView<LoanView, LoanViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="LoanView"/> class.</summary>
	public LoanView() => InitializeComponent();
}
