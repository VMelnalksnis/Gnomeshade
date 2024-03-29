﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Counterparties;

namespace Gnomeshade.Desktop.Views.Counterparties;

/// <inheritdoc cref="CounterpartyMergeViewModel"/>
public sealed partial class CounterpartyMergeView : UserControl, IView<CounterpartyMergeView, CounterpartyMergeViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="CounterpartyMergeView"/> class.</summary>
	public CounterpartyMergeView()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
