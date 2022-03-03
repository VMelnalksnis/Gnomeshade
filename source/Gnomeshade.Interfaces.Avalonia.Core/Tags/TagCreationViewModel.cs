// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Tags;

namespace Gnomeshade.Interfaces.Avalonia.Core.Tags;

/// <summary>Creating or updating an existing tag.</summary>
public sealed class TagCreationViewModel : ViewModelBase
{
	private readonly ITagClient _tagClient;
	private readonly Guid? _originalId;

	private string _name;
	private string? _description;

	private TagCreationViewModel(ITagClient tagClient, Tag? tag = null)
	{
		_tagClient = tagClient;
		_originalId = tag?.Id;

		_name = tag?.Name ?? string.Empty;
		_description = tag?.Description;
	}

	/// <summary>Raised when a new tag has been created.</summary>
	public event EventHandler<TagCreatedEventArgs>? TagCreated;

	/// <summary>Gets or sets the name of the tag.</summary>
	public string Name
	{
		get => _name;
		set => SetAndNotifyWithGuard(ref _name, value, nameof(Name), nameof(CanSave));
	}

	/// <summary>Gets or sets the description of the tag.</summary>
	public string? Description
	{
		get => _description;
		set => SetAndNotify(ref _description, value);
	}

	/// <summary>Gets a value indicating whether the tag can be saved.</summary>
	public bool CanSave => !string.IsNullOrWhiteSpace(Name);

	/// <summary>Asynchronously creates a new instance of the <see cref="TagCreationViewModel"/> class.</summary>
	/// <param name="tagClient">API client for getting tag data.</param>
	/// <param name="originalId">The id of the tag to edit.</param>
	/// <returns>A new instance of the <see cref="TagCreationViewModel"/> class.</returns>
	public static async Task<TagCreationViewModel> CreateAsync(ITagClient tagClient, Guid? originalId = null)
	{
		if (originalId is null)
		{
			return new(tagClient);
		}

		var originalTag = await tagClient.GetTagAsync(originalId.Value);
		return new(tagClient, originalTag);
	}

	/// <summary>Creates new tag from information in this viewmodel.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task SaveAsync()
	{
		var tagCreationModel = new TagCreation
		{
			Name = Name,
			Description = Description,
		};

		var tagId = _originalId ?? await _tagClient.CreateTagAsync(tagCreationModel);
		if (_originalId is not null)
		{
			await _tagClient.PutTagAsync(tagId, tagCreationModel);
		}

		TagCreated?.Invoke(this, new(tagId));
	}
}
