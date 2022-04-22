// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Tags;

namespace Gnomeshade.Interfaces.Avalonia.Core.Categories;

/// <summary>Creating or updating an existing category.</summary>
public sealed class CategoryCreationViewModel : UpsertionViewModel
{
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly Guid? _originalId;

	private string? _name;
	private string? _description;

	private CategoryCreationViewModel(IGnomeshadeClient gnomeshadeClient, Category? category = null)
		: base(gnomeshadeClient)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_originalId = category?.Id;

		_name = category?.Name;
		_description = category?.Description;
	}

	/// <summary>Gets or sets the name of the category.</summary>
	public string? Name
	{
		get => _name;
		set => SetAndNotifyWithGuard(ref _name, value, nameof(Name), nameof(CanSave));
	}

	/// <summary>Gets or sets the description of the category.</summary>
	public string? Description
	{
		get => _description;
		set => SetAndNotify(ref _description, value);
	}

	/// <inheritdoc />
	public override bool CanSave => !string.IsNullOrWhiteSpace(Name);

	/// <summary>Asynchronously creates a new instance of the <see cref="CategoryCreationViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">API client for getting category data.</param>
	/// <param name="originalId">The id of the category to edit.</param>
	/// <returns>A new instance of the <see cref="CategoryCreationViewModel"/> class.</returns>
	public static async Task<CategoryCreationViewModel> CreateAsync(IGnomeshadeClient gnomeshadeClient, Guid? originalId = null)
	{
		if (originalId is null)
		{
			return new(gnomeshadeClient);
		}

		var originalTag = await gnomeshadeClient.GetCategoryAsync(originalId.Value);
		return new(gnomeshadeClient, originalTag);
	}

	/// <inheritdoc />
	protected override async Task<Guid> SaveValidatedAsync()
	{
		var categoryCreationModel = new CategoryCreation
		{
			Name = Name!,
			Description = Description,
		};

		var categoryId = _originalId ?? await _gnomeshadeClient.CreateCategoryAsync(categoryCreationModel);
		if (_originalId is not null)
		{
			await _gnomeshadeClient.PutCategoryAsync(categoryId, categoryCreationModel);
		}

		return categoryId;
	}
}
