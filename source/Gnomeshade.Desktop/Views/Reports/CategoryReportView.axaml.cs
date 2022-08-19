// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Reports;

namespace Gnomeshade.Desktop.Views.Reports;

/// <inheritdoc cref="CategoryReportViewModel"/>
public sealed class CategoryReportView : UserControl, IView<CategoryReportView, CategoryReportViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="CategoryReportView"/> class.</summary>
	public CategoryReportView()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
