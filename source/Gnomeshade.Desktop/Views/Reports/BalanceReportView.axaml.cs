// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Reports;

namespace Gnomeshade.Desktop.Views.Reports;

/// <inheritdoc cref="BalanceReportViewModel"/>
public sealed partial class BalanceReportView : UserControl, IView<BalanceReportView, BalanceReportViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="BalanceReportView"/> class.</summary>
	public BalanceReportView()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
