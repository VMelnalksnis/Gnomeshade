// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Reports;

namespace Gnomeshade.Desktop.Views.Reports;

/// <inheritdoc cref="ProductReportViewModel"/>
public sealed class ProductReportView : UserControl, IView<ProductReportViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="ProductReportView"/> class.</summary>
	public ProductReportView()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
