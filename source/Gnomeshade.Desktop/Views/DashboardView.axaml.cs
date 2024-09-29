// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;

using Gnomeshade.Avalonia.Core;

namespace Gnomeshade.Desktop.Views;

/// <inheritdoc cref="DashboardViewModel" />
public sealed partial class DashboardView : UserControl, IView<DashboardView, DashboardViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="DashboardView"/> class.</summary>
	public DashboardView() => InitializeComponent();
}
