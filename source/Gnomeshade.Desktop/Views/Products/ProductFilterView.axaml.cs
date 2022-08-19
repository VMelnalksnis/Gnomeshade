// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Products;

namespace Gnomeshade.Desktop.Views.Products;

/// <inheritdoc cref="ProductFilter" />
public sealed class ProductFilterView : UserControl, IView<ProductFilterView, ProductFilter>
{
	/// <summary>Initializes a new instance of the <see cref="ProductFilterView"/> class.</summary>
	public ProductFilterView()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
