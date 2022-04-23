// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Products;

namespace Gnomeshade.Interfaces.Avalonia.Core.Products;

/// <summary>Creating or updating an existing category.</summary>
public sealed class CategoryCreationViewModel : UpsertionViewModel
{
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly Guid? _originalId;

	private string? _name;
	private string? _description;
	private Category? _selectedCategory;
	private List<Category> _categories;

	private CategoryCreationViewModel(IGnomeshadeClient gnomeshadeClient, Category? category = null)
		: base(gnomeshadeClient)
	{
		_categories = new();
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

	/// <summary>Gets or sets the category of this product.</summary>
	public Category? SelectedCategory
	{
		get => _selectedCategory;
		set => SetAndNotify(ref _selectedCategory, value);
	}

	/// <summary>Gets a collection of all available categories.</summary>
	public List<Category> Categories
	{
		get => _categories;
		private set => SetAndNotify(ref _categories, value);
	}

	/// <summary>Gets a delegate for formatting a category in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> CategorySelector => AutoCompleteSelectors.Category;

	/// <inheritdoc />
	public override bool CanSave => !string.IsNullOrWhiteSpace(Name);

	/// <summary>Asynchronously creates a new instance of the <see cref="CategoryCreationViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">API client for getting category data.</param>
	/// <param name="originalId">The id of the category to edit.</param>
	/// <returns>A new instance of the <see cref="CategoryCreationViewModel"/> class.</returns>
	public static async Task<CategoryCreationViewModel> CreateAsync(IGnomeshadeClient gnomeshadeClient, Guid? originalId = null)
	{
		var category = originalId is null
			? null
			: await gnomeshadeClient.GetCategoryAsync(originalId.Value);

		var viewModel = new CategoryCreationViewModel(gnomeshadeClient, category);
		await viewModel.RefreshAsync().ConfigureAwait(false);
		return viewModel;
	}

	/// <inheritdoc />
	public override async Task RefreshAsync()
	{
		var categories = await _gnomeshadeClient.GetCategoriesAsync().ConfigureAwait(false);
		var editedCategory = categories.SingleOrDefault(category => category.Id == _originalId);
		if (editedCategory is not null)
		{
			categories.Remove(editedCategory);
		}

		Categories = categories;
	}

	/// <inheritdoc />
	protected override async Task<Guid> SaveValidatedAsync()
	{
		var categoryCreationModel = new CategoryCreation
		{
			Name = Name!,
			Description = Description,
			CategoryId = SelectedCategory?.Id,
		};

		var categoryId = _originalId ?? await _gnomeshadeClient.CreateCategoryAsync(categoryCreationModel);
		if (_originalId is not null)
		{
			await _gnomeshadeClient.PutCategoryAsync(categoryId, categoryCreationModel);
		}

		return categoryId;
	}
}
