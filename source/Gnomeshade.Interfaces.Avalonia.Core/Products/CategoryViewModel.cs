// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Client;

namespace Gnomeshade.Interfaces.Avalonia.Core.Products;

/// <summary>An overview of all categories.</summary>
public sealed class CategoryViewModel : OverviewViewModel<CategoryRow, CategoryCreationViewModel>
{
	private readonly IGnomeshadeClient _gnomeshadeClient;

	private CategoryCreationViewModel _category;
	private ObservableCollection<CategoryNode> _nodes;

	private CategoryViewModel(IGnomeshadeClient gnomeshadeClient, CategoryCreationViewModel categoryCreationViewModel)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_category = categoryCreationViewModel;
		_nodes = new();

		Details.Upserted += OnCategoryUpserted;
		PropertyChanged += OnPropertyChanged;
	}

	/// <inheritdoc />
	public override CategoryCreationViewModel Details
	{
		get => _category;
		set
		{
			Details.Upserted -= OnCategoryUpserted;
			SetAndNotify(ref _category, value);
			Details.Upserted += OnCategoryUpserted;
		}
	}

	/// <summary>Gets all categories in a hierarchical structure.</summary>
	public ObservableCollection<CategoryNode> Nodes
	{
		get => _nodes;
		private set => SetAndNotify(ref _nodes, value);
	}

	/// <summary>Asynchronously creates a new instance of the <see cref="CategoryViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">API client for getting category data.</param>
	/// <returns>A new instance of the <see cref="CategoryViewModel"/> class.</returns>
	public static async Task<CategoryViewModel> CreateAsync(IGnomeshadeClient gnomeshadeClient)
	{
		var creationViewModel = await CategoryCreationViewModel.CreateAsync(gnomeshadeClient);
		var viewModel = new CategoryViewModel(gnomeshadeClient, creationViewModel);
		await viewModel.RefreshAsync().ConfigureAwait(false);
		return viewModel;
	}

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		var categories = await _gnomeshadeClient.GetCategoriesAsync();
		var categoryRows = categories.Select(category =>
		{
			var c = categories.SingleOrDefault(cc => cc.Id == category.CategoryId);
			return new CategoryRow(category.Id, category.Name, category.Description, c?.Name);
		}).ToList();

		var rootNodes = categories.Where(category => category.CategoryId is null).Select(category => CategoryNode.FromCategory(category, categories));
		Nodes = new(rootNodes);

		var sortDescriptions = DataGridView.SortDescriptions;
		Rows = new(categoryRows);
		DataGridView.SortDescriptions.AddRange(sortDescriptions);

		Details = Task.Run(() => CategoryCreationViewModel.CreateAsync(_gnomeshadeClient)).Result;
	}

	/// <inheritdoc />
	protected override async Task DeleteAsync(CategoryRow row)
	{
		await _gnomeshadeClient.DeleteCategoryAsync(row.Id).ConfigureAwait(false);
		await Refresh().ConfigureAwait(false);
	}

	private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(Selected))
		{
			Details = Task.Run(() => CategoryCreationViewModel.CreateAsync(_gnomeshadeClient, Selected?.Id)).Result;
		}
	}

	private void OnCategoryUpserted(object? sender, UpsertedEventArgs e)
	{
		Task.Run(Refresh).GetAwaiter().GetResult();
	}
}
