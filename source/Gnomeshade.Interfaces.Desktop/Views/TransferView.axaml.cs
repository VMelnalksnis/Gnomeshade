﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Gnomeshade.Interfaces.Avalonia.Core;
using Gnomeshade.Interfaces.Avalonia.Core.Transactions.Transfers;

namespace Gnomeshade.Interfaces.Desktop.Views;

/// <inheritdoc cref="TransferViewModel" />
public sealed class TransferView : UserControl, IView<TransferViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="TransferView"/> class.</summary>
	public TransferView()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
