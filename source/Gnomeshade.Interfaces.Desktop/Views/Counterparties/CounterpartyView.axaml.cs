// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Counterparties;

namespace Gnomeshade.Interfaces.Desktop.Views.Counterparties;

/// <inheritdoc cref="CounterpartyViewModel"/>
public sealed class CounterpartyView : UserControl, IView<CounterpartyViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="CounterpartyView"/> class.</summary>
	public CounterpartyView()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
