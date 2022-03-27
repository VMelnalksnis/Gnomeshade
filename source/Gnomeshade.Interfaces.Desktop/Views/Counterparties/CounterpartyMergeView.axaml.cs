// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Gnomeshade.Interfaces.Avalonia.Core;
using Gnomeshade.Interfaces.Avalonia.Core.Counterparties;

namespace Gnomeshade.Interfaces.Desktop.Views.Counterparties;

/// <summary>Merge two counterparties.</summary>
public sealed class CounterpartyMergeView : UserControl, IView<CounterpartyMergeViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="CounterpartyMergeView"/> class.</summary>
	public CounterpartyMergeView()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
