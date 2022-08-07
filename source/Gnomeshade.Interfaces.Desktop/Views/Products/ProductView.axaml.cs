// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Products;

namespace Gnomeshade.Interfaces.Desktop.Views.Products;

/// <inheritdoc cref="ProductViewModel"/>
public sealed class ProductView : UserControl, IView<ProductViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="ProductView"/> class.</summary>
	public ProductView()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
