// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Collections;

using Gnomeshade.Interfaces.WebApi.Client;

namespace Gnomeshade.Interfaces.Avalonia.Core.Categories;

/// <summary>An overview of all categories.</summary>
public sealed class CategoryViewModel : ViewModelBase
{
	private readonly IGnomeshadeClient _gnomeshadeClient;

	private DataGridItemCollectionView<CategoryRow> _categoryDataGridView;
	private CategoryRow? _selectedTag;
	private CategoryCreationViewModel _category;

	private CategoryViewModel(IGnomeshadeClient gnomeshadeClient, IEnumerable<CategoryRow> categoryRows, CategoryCreationViewModel categoryCreationViewModel)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_category = categoryCreationViewModel;
		Category.Upserted += OnCategoryUpserted;

		_categoryDataGridView = new(categoryRows);
		PropertyChanged += OnPropertyChanged;
	}

	/// <summary>Gets a grid view of all categories.</summary>
	public DataGridCollectionView DataGridView => TagDataGridView;

	/// <summary>Gets a typed collection of categories in <see cref="DataGridView"/>.</summary>
	public DataGridItemCollectionView<CategoryRow> TagDataGridView
	{
		get => _categoryDataGridView;
		private set => SetAndNotifyWithGuard(ref _categoryDataGridView, value, nameof(TagDataGridView), nameof(DataGridView));
	}

	/// <summary>Gets or sets the currently selected category from <see cref="TagDataGridView"/>.</summary>
	public CategoryRow? SelectedTag
	{
		get => _selectedTag;
		set => SetAndNotify(ref _selectedTag, value);
	}

	/// <summary>Gets the current category creation view model.</summary>
	public CategoryCreationViewModel Category
	{
		get => _category;
		private set
		{
			Category.Upserted -= OnCategoryUpserted;
			SetAndNotify(ref _category, value);
			Category.Upserted += OnCategoryUpserted;
		}
	}

	/// <summary>Asynchronously creates a new instance of the <see cref="CategoryViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">API client for getting category data.</param>
	/// <returns>A new instance of the <see cref="CategoryViewModel"/> class.</returns>
	public static async Task<CategoryViewModel> CreateAsync(IGnomeshadeClient gnomeshadeClient)
	{
		var categories = await gnomeshadeClient.GetCategoriesAsync();
		var categoryRows = categories.Select(category => new CategoryRow(category.Id, category.Name, category.Description));
		var categoryCreationViewModel = await CategoryCreationViewModel.CreateAsync(gnomeshadeClient);
		return new(gnomeshadeClient, categoryRows, categoryCreationViewModel);
	}

	private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(SelectedTag))
		{
			Category = Task.Run(() => CategoryCreationViewModel.CreateAsync(_gnomeshadeClient, SelectedTag?.Id)).Result;
		}
	}

	private void OnCategoryUpserted(object? sender, UpsertedEventArgs e)
	{
		var categories = _gnomeshadeClient.GetCategoriesAsync().Result;
		var categoryRows = categories.Select(category => new CategoryRow(category.Id, category.Name, category.Description));

		var sortDescriptions = DataGridView.SortDescriptions;

		TagDataGridView = new(categoryRows);
		DataGridView.SortDescriptions.AddRange(sortDescriptions);
		Category = Task.Run(() => CategoryCreationViewModel.CreateAsync(_gnomeshadeClient)).Result;
	}
}
