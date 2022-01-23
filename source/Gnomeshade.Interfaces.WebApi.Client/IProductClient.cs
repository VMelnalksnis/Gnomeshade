// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Models.Products;

namespace Gnomeshade.Interfaces.WebApi.Client;

/// <summary>
/// Provides typed interface for using the import product API.
/// </summary>
public interface IProductClient
{
	/// <summary>
	/// Gets all products.
	/// </summary>
	/// <returns>A collection with all products.</returns>
	Task<List<Product>> GetProductsAsync();

	/// <summary>
	/// Gets the specified product.
	/// </summary>
	/// <param name="id">The id of the product to get.</param>
	/// <returns>The product with the specified id.</returns>
	Task<Product> GetProductAsync(Guid id);

	/// <summary>
	/// Gets all units.
	/// </summary>
	/// <returns>A collection with all units.</returns>
	Task<List<Unit>> GetUnitsAsync();

	/// <summary>
	/// Creates a new product or replaces an existing one if one exists with the specified id.
	/// </summary>
	/// <param name="id">The id of the product.</param>
	/// <param name="product">The product to create or replace.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task PutProductAsync(Guid id, ProductCreationModel product);

	/// <summary>
	/// Creates a new unit.
	/// </summary>
	/// <param name="unit">The unit to create.</param>
	/// <returns>The id of the created unit.</returns>
	Task<Guid> CreateUnitAsync(UnitCreationModel unit);
}
