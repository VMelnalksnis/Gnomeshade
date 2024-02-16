// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;

namespace Gnomeshade.Avalonia.Core.Products;

/// <summary>An overview of all categories.</summary>
public sealed class CategoryViewModel : OverviewViewModel<CategoryRow, CategoryUpsertionViewModel>
{
	private readonly IGnomeshadeClient _gnomeshadeClient;

	private CategoryUpsertionViewModel _details;
	private ObservableCollection<CategoryNode> _nodes;

	/// <summary>Initializes a new instance of the <see cref="CategoryViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	public CategoryViewModel(IActivityService activityService, IGnomeshadeClient gnomeshadeClient)
		: base(activityService)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_details = new(activityService, _gnomeshadeClient, null);
		_nodes = [];

		Filter = new(activityService);

		Filter.PropertyChanged += FilterOnPropertyChanged;
		Details.Upserted += OnCategoryUpserted;
	}

	/// <inheritdoc />
	public override CategoryUpsertionViewModel Details
	{
		get => _details;
		set
		{
			Details.Upserted -= OnCategoryUpserted;
			SetAndNotify(ref _details, value);
			Details.Upserted += OnCategoryUpserted;
		}
	}

	/// <summary>Gets all categories in a hierarchical structure.</summary>
	public ObservableCollection<CategoryNode> Nodes
	{
		get => _nodes;
		private set => SetAndNotify(ref _nodes, value);
	}

	/// <summary>Gets the category filter.</summary>
	public CategoryFilter Filter { get; }

	/// <inheritdoc />
	public override Task UpdateSelection()
	{
		Details = new(ActivityService, _gnomeshadeClient, Selected?.Id);
		return Details.RefreshAsync();
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

		var rootNodes = categories.Where(category => category.CategoryId is null).Select(category => CategoryNode.FromCategory(category, categories)).ToList();
		Nodes = new(rootNodes);

		var sortDescriptions = DataGridView.SortDescriptions;
		var selected = Selected;
		Rows = new(categoryRows);

		Filter.Nodes = rootNodes;
		Filter.Categories = categories;

		DataGridView.SortDescriptions.AddRange(sortDescriptions);
		DataGridView.Filter = Filter.Filter;
		Selected = Rows.SingleOrDefault(row => row.Id == selected?.Id);

		if (Details.Categories.Count is 0)
		{
			await Details.RefreshAsync();
		}
	}

	/// <inheritdoc />
	protected override async Task DeleteAsync(CategoryRow row)
	{
		await _gnomeshadeClient.DeleteCategoryAsync(row.Id);
		await RefreshAsync();
	}

	private async void OnCategoryUpserted(object? sender, UpsertedEventArgs e)
	{
		await RefreshAsync();
	}

	private void FilterOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is not nameof(CategoryFilter.Categories))
		{
			DataGridView.Refresh();
		}
	}
}
