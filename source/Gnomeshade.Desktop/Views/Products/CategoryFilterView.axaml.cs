// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Products;

namespace Gnomeshade.Desktop.Views.Products;

/// <inheritdoc cref="CategoryFilter" />
public sealed partial class CategoryFilterView : UserControl, IView<CategoryFilterView, CategoryFilter>
{
	/// <summary>Initializes a new instance of the <see cref="CategoryFilterView"/> class.</summary>
	public CategoryFilterView()
	{
		InitializeComponent();
	}
}
