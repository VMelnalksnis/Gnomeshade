// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Loans;

namespace Gnomeshade.Desktop.Views.Loans;

/// <inheritdoc cref="LoanUpsertionViewModel" />
public sealed partial class LoanUpsertionView : UserControl, IView<LoanUpsertionView, LoanUpsertionViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="LoanUpsertionView"/> class.</summary>
	public LoanUpsertionView() => InitializeComponent();
}
