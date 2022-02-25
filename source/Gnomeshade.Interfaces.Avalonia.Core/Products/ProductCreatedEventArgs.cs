// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Gnomeshade.Interfaces.Avalonia.Core.Products;

/// <summary>
/// Event arguments for the <see cref="ProductCreationViewModel.ProductCreated"/> event.
/// </summary>
public sealed class ProductCreatedEventArgs : EventArgs
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ProductCreatedEventArgs"/> class.
	/// </summary>
	/// <param name="productId">The id of the created product.</param>
	public ProductCreatedEventArgs(Guid productId)
	{
		ProductId = productId;
	}

	/// <summary>
	/// Gets the id of the product that was created.
	/// </summary>
	public Guid ProductId { get; }
}
