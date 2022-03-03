// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Models.Tags;

namespace Gnomeshade.Interfaces.WebApi.Client;

/// <summary>Provides typed interface for using the tag API.</summary>
public interface ITagClient
{
	/// <summary>Gets all tags.</summary>
	/// <returns>A collection with all tags.</returns>
	Task<List<Tag>> GetTagsAsync();

	/// <summary>Gets the tag with the specified id.</summary>
	/// <param name="id">The id by which to search for the tag.</param>
	/// <returns>The tag with the specified id.</returns>
	Task<Tag> GetTagAsync(Guid id);

	/// <summary>Creates a new tag.</summary>
	/// <param name="tag">The tag to create.</param>
	/// <returns>The id of the created tag.</returns>
	Task<Guid> CreateTagAsync(TagCreation tag);

	/// <summary>Creates a new tag, or replaces and existing one if one exists with the specified id.</summary>
	/// <param name="id">The id of the tag.</param>
	/// <param name="tag">The tag to create or update.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task PutTagAsync(Guid id, TagCreation tag);

	/// <summary>Deletes the specified tag.</summary>
	/// <param name="id">The id of the tag to delete.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task DeleteTagAsync(Guid id);

	/// <summary>Gets all tags of the specified tag.</summary>
	/// <param name="id">The id of the tag for which to get the tags.</param>
	/// <returns>All tags of the specified tag.</returns>
	Task<List<Tag>> GetTagTagsAsync(Guid id);

	/// <summary>Tags the specified tag with the specified tag.</summary>
	/// <param name="id">The id of the tag to tag.</param>
	/// <param name="tagId">The id of the tag to add.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task TagTagAsync(Guid id, Guid tagId);

	/// <summary>Removes the specified tag from the specified tag.</summary>
	/// <param name="id">The id of the tag to untag.</param>
	/// <param name="tagId">The id of the tag to remove.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task UntagTagAsync(Guid id, Guid tagId);
}
