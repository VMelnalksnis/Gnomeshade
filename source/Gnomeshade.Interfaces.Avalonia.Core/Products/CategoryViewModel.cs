// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Client;

namespace Gnomeshade.Interfaces.Avalonia.Core.Products;

/// <summary>An overview of all categories.</summary>
public sealed class CategoryViewModel : OverviewViewModel<CategoryRow, CategoryUpsertionViewModel>
{
	private readonly IGnomeshadeClient _gnomeshadeClient;

	private CategoryUpsertionViewModel _details;
	private ObservableCollection<CategoryNode> _nodes;

	/// <summary>Initializes a new instance of the <see cref="CategoryViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	public CategoryViewModel(IGnomeshadeClient gnomeshadeClient)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_details = new(_gnomeshadeClient, null);
		_nodes = new();

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

	/// <inheritdoc />
	public override Task UpdateSelection()
	{
		Details = new(_gnomeshadeClient, Selected?.Id);
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

		var rootNodes = categories.Where(category => category.CategoryId is null).Select(category => CategoryNode.FromCategory(category, categories));
		Nodes = new(rootNodes);

		var sortDescriptions = DataGridView.SortDescriptions;
		var selected = Selected;
		Rows = new(categoryRows);
		DataGridView.SortDescriptions.AddRange(sortDescriptions);
		Selected = Rows.SingleOrDefault(row => row.Id == selected?.Id);

		if (Details.Categories.Count is 0)
		{
			await Details.RefreshAsync();
		}
	}

	/// <inheritdoc />
	protected override async Task DeleteAsync(CategoryRow row)
	{
		await _gnomeshadeClient.DeleteCategoryAsync(row.Id).ConfigureAwait(false);
		await RefreshAsync();
	}

	private async void OnCategoryUpserted(object? sender, UpsertedEventArgs e)
	{
		await RefreshAsync();
	}
}
