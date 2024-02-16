// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Counterparties;

namespace Gnomeshade.Desktop.Views.Counterparties;

/// <inheritdoc cref="CounterpartyFilter" />
public sealed partial class CounterpartyFilterView : UserControl, IView<CounterpartyFilterView, CounterpartyFilter>
{
	/// <summary>Initializes a new instance of the <see cref="CounterpartyFilterView"/> class.</summary>
	public CounterpartyFilterView()
	{
		InitializeComponent();
	}
}
