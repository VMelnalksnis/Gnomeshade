﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Transactions.Loans;

namespace Gnomeshade.Desktop.Views.Transactions.Loans;

/// <inheritdoc cref="LoanPaymentUpsertionViewModel"/>
public sealed partial class PlannedLoanUpsertionView : UserControl, IView<PlannedLoanUpsertionView, PlannedLoanPaymentUpsertionViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="PlannedLoanUpsertionView"/> class.</summary>
	public PlannedLoanUpsertionView() => InitializeComponent();
}
