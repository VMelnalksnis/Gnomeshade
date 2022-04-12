﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Markup.Xaml;

using Gnomeshade.Interfaces.Avalonia.Core;
using Gnomeshade.Interfaces.Avalonia.Core.Products;

namespace Gnomeshade.Interfaces.Desktop.Views.Products;

/// <summary>Create or update a single product.</summary>
public sealed class ProductCreationView : FocusOnInitUserControl, IView<ProductCreationViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="ProductCreationView"/> class.</summary>
	public ProductCreationView()
	{
		AvaloniaXamlLoader.Load(this);
		Focus("ProductName");
	}
}