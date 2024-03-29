﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Transactions;

namespace Gnomeshade.Desktop.Views.Transactions;

/// <inheritdoc cref="TransactionUpsertionViewModel" />
public sealed partial class TransactionUpsertionView : UserControl, IView<TransactionUpsertionView, TransactionUpsertionViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="TransactionUpsertionView"/> class.</summary>
	public TransactionUpsertionView()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
