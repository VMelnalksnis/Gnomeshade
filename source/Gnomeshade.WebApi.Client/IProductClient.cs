// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Models.Products;
using Gnomeshade.WebApi.Models.Transactions;

namespace Gnomeshade.WebApi.Client;

/// <summary>Provides typed interface for using the import product API.</summary>
public interface IProductClient
{
	/// <summary>Gets all products.</summary>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>A collection with all products.</returns>
	Task<List<Product>> GetProductsAsync(CancellationToken cancellationToken = default);

	/// <summary>Gets the specified product.</summary>
	/// <param name="id">The id of the product to get.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>The product with the specified id.</returns>
	Task<Product> GetProductAsync(Guid id, CancellationToken cancellationToken = default);

	/// <summary>Creates a new product or replaces an existing one if one exists with the specified id.</summary>
	/// <param name="id">The id of the product.</param>
	/// <param name="product">The product to create or replace.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task PutProductAsync(Guid id, ProductCreation product);

	/// <summary>Deletes the specified transaction.</summary>
	/// <param name="id">The id of the transaction to delete.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task DeleteProductAsync(Guid id);

	/// <summary>Gets the specified unit.</summary>
	/// <param name="id">The id of the unit to get.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>The unit with the specified id.</returns>
	Task<Unit> GetUnitAsync(Guid id, CancellationToken cancellationToken = default);

	/// <summary>Gets all units.</summary>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>A collection with all units.</returns>
	Task<List<Unit>> GetUnitsAsync(CancellationToken cancellationToken = default);

	/// <summary>Creates a new unit or replaces an existing one if one exists with the specified id.</summary>
	/// <param name="id">The id of the unit.</param>
	/// <param name="unit">The unit to create or replace.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task PutUnitAsync(Guid id, UnitCreation unit);

	/// <summary>Gets all categories.</summary>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>A collection with all categories.</returns>
	Task<List<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default);

	/// <summary>Gets the category with the specified id.</summary>
	/// <param name="id">The id by which to search for the category.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>The category with the specified id.</returns>
	Task<Category> GetCategoryAsync(Guid id, CancellationToken cancellationToken = default);

	/// <summary>Creates a new category.</summary>
	/// <param name="category">The category to create.</param>
	/// <returns>The id of the created category.</returns>
	Task<Guid> CreateCategoryAsync(CategoryCreation category);

	/// <summary>Creates a new category, or replaces and existing one if one exists with the specified id.</summary>
	/// <param name="id">The id of the category.</param>
	/// <param name="category">The category to create or update.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task PutCategoryAsync(Guid id, CategoryCreation category);

	/// <summary>Deletes the specified category.</summary>
	/// <param name="id">The id of the category to delete.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task DeleteCategoryAsync(Guid id);

	/// <summary>Gets all purchases of the specified product.</summary>
	/// <param name="id">The id of the product for which to get all the purchases.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>All purchases of the specified product.</returns>
	Task<List<Purchase>> GetProductPurchasesAsync(Guid id, CancellationToken cancellationToken = default);
}
