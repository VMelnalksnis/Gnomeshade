// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Tags;

namespace Gnomeshade.Interfaces.Avalonia.Core.Tags;

/// <summary>Creating or updating an existing tag.</summary>
public sealed class TagCreationViewModel : UpsertionViewModel
{
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly Guid? _originalId;

	private string? _name;
	private string? _description;

	private TagCreationViewModel(IGnomeshadeClient gnomeshadeClient, Tag? tag = null)
		: base(gnomeshadeClient)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_originalId = tag?.Id;

		_name = tag?.Name;
		_description = tag?.Description;
	}

	/// <summary>Gets or sets the name of the tag.</summary>
	public string? Name
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

	/// <inheritdoc />
	public override bool CanSave => !string.IsNullOrWhiteSpace(Name);

	/// <summary>Asynchronously creates a new instance of the <see cref="TagCreationViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">API client for getting tag data.</param>
	/// <param name="originalId">The id of the tag to edit.</param>
	/// <returns>A new instance of the <see cref="TagCreationViewModel"/> class.</returns>
	public static async Task<TagCreationViewModel> CreateAsync(IGnomeshadeClient gnomeshadeClient, Guid? originalId = null)
	{
		if (originalId is null)
		{
			return new(gnomeshadeClient);
		}

		var originalTag = await gnomeshadeClient.GetTagAsync(originalId.Value);
		return new(gnomeshadeClient, originalTag);
	}

	/// <inheritdoc />
	protected override async Task<Guid> SaveValidatedAsync()
	{
		var tagCreationModel = new TagCreation
		{
			Name = Name!,
			Description = Description,
		};

		var tagId = _originalId ?? await _gnomeshadeClient.CreateTagAsync(tagCreationModel);
		if (_originalId is not null)
		{
			await _gnomeshadeClient.PutTagAsync(tagId, tagCreationModel);
		}

		return tagId;
	}
}
