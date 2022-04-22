// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Models.Tags;

namespace Gnomeshade.Interfaces.WebApi.Client;

/// <summary>Provides typed interface for using the tag API.</summary>
public interface ICategoryClient
{
	/// <summary>Gets all tags.</summary>
	/// <returns>A collection with all tags.</returns>
	Task<List<Category>> GetCategoriesAsync();

	/// <summary>Gets the tag with the specified id.</summary>
	/// <param name="id">The id by which to search for the tag.</param>
	/// <returns>The tag with the specified id.</returns>
	Task<Category> GetCategoryAsync(Guid id);

	/// <summary>Creates a new tag.</summary>
	/// <param name="category">The tag to create.</param>
	/// <returns>The id of the created tag.</returns>
	Task<Guid> CreateCategoryAsync(CategoryCreation category);

	/// <summary>Creates a new tag, or replaces and existing one if one exists with the specified id.</summary>
	/// <param name="id">The id of the tag.</param>
	/// <param name="category">The tag to create or update.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task PutCategoryAsync(Guid id, CategoryCreation category);

	/// <summary>Deletes the specified tag.</summary>
	/// <param name="id">The id of the tag to delete.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task DeleteCategoryAsync(Guid id);
}
