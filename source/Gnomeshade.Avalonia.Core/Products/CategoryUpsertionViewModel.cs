// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;

using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Products;

namespace Gnomeshade.Avalonia.Core.Products;

/// <summary>Creating or updating an existing category.</summary>
public sealed class CategoryUpsertionViewModel : UpsertionViewModel
{
	private Guid? _id;
	private string? _name;
	private string? _description;
	private Category? _selectedCategory;
	private List<Category> _categories;

	/// <summary>Initializes a new instance of the <see cref="CategoryUpsertionViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">API client for getting category data.</param>
	/// <param name="id">The id of the category to edit.</param>
	public CategoryUpsertionViewModel(IGnomeshadeClient gnomeshadeClient, Guid? id)
		: base(gnomeshadeClient)
	{
		_id = id;

		_categories = new();
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

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		var categories = await GnomeshadeClient.GetCategoriesAsync().ConfigureAwait(false);
		var editedCategory = categories.SingleOrDefault(category => category.Id == _id);
		if (editedCategory is not null)
		{
			categories.Remove(editedCategory);
			Name = editedCategory.Name;
			Description = editedCategory.Description;
			SelectedCategory = categories.SingleOrDefault(category => category.Id == editedCategory.CategoryId);
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

		_id ??= Guid.NewGuid();
		await GnomeshadeClient.PutCategoryAsync(_id.Value, categoryCreationModel).ConfigureAwait(false);
		return _id.Value;
	}
}
