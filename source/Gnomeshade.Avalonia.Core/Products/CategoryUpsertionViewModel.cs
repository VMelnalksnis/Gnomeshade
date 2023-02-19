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

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Products;

/// <summary>Creating or updating an existing category.</summary>
public sealed partial class CategoryUpsertionViewModel : UpsertionViewModel
{
	/// <summary>Gets or sets the name of the category.</summary>
	[Notify]
	private string? _name;

	/// <summary>Gets or sets the description of the category.</summary>
	[Notify]
	private string? _description;

	/// <summary>Gets or sets the category of this product.</summary>
	[Notify]
	private Category? _selectedCategory;

	/// <summary>Gets a collection of all available categories.</summary>
	[Notify(Setter.Private)]
	private List<Category> _categories;

	/// <summary>Initializes a new instance of the <see cref="CategoryUpsertionViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">API client for getting category data.</param>
	/// <param name="id">The id of the category to edit.</param>
	public CategoryUpsertionViewModel(IActivityService activityService, IGnomeshadeClient gnomeshadeClient, Guid? id)
		: base(activityService, gnomeshadeClient)
	{
		Id = id;

		_categories = new();
	}

	/// <summary>Gets a delegate for formatting a category in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> CategorySelector => AutoCompleteSelectors.Category;

	/// <inheritdoc />
	public override bool CanSave => !string.IsNullOrWhiteSpace(Name);

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		var categories = await GnomeshadeClient.GetCategoriesAsync();
		var editedCategory = categories.SingleOrDefault(category => category.Id == Id);
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

		var id = Id ?? Guid.NewGuid();
		await GnomeshadeClient.PutCategoryAsync(id, categoryCreationModel);
		return id;
	}
}
