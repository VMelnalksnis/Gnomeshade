﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Transactions.Controls;

namespace Gnomeshade.Desktop.Views.Transactions.Controls;

/// <inheritdoc cref="TransactionProperties"/>
public sealed partial class TransactionPropertiesView : UserControl, IView<TransactionPropertiesView, TransactionProperties>
{
	/// <summary>Initializes a new instance of the <see cref="TransactionPropertiesView"/> class.</summary>
	public TransactionPropertiesView()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
