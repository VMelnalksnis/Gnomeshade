// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Interfaces.Avalonia.Core.Imports;

namespace Gnomeshade.Interfaces.Avalonia.Core.Products;

/// <summary>
/// Event arguments for <see cref="ImportViewModel.ProductSelected"/> event.
/// </summary>
public sealed class ProductSelectedEventArgs : EventArgs
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ProductSelectedEventArgs"/> class.
	/// </summary>
	/// <param name="productId">The id of the selected product.</param>
	public ProductSelectedEventArgs(Guid productId)
	{
		ProductId = productId;
	}

	/// <summary>
	/// Gets the id of the selected product.
	/// </summary>
	public Guid ProductId { get; }
}
