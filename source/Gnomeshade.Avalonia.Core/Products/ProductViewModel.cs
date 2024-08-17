// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;

using NodaTime;

namespace Gnomeshade.Avalonia.Core.Products;

/// <summary>Overview and editing of all products.</summary>
public sealed class ProductViewModel : OverviewViewModel<ProductRow, ProductUpsertionViewModel>
{
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider;

	private ProductUpsertionViewModel _details;

	/// <summary>Initializes a new instance of the <see cref="ProductViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	/// <param name="dateTimeZoneProvider">Time zone provider for localizing instants to local time.</param>
	public ProductViewModel(
		IActivityService activityService,
		IGnomeshadeClient gnomeshadeClient,
		IDateTimeZoneProvider dateTimeZoneProvider)
		: base(activityService)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_dateTimeZoneProvider = dateTimeZoneProvider;
		_details = new(activityService, _gnomeshadeClient, _dateTimeZoneProvider, null);

		Filter = new(activityService);

		Filter.PropertyChanged += FilterOnPropertyChanged;
		Details.Upserted += OnProductUpserted;
	}

	/// <inheritdoc />
	public override ProductUpsertionViewModel Details
	{
		get => _details;
		set
		{
			Details.Upserted -= OnProductUpserted;
			SetAndNotify(ref _details, value);
			Details.Upserted += OnProductUpserted;
		}
	}

	/// <summary>Gets the product filter.</summary>
	public ProductFilter Filter { get; }

	/// <inheritdoc />
	public override Task UpdateSelection()
	{
		Details = new(ActivityService, _gnomeshadeClient, _dateTimeZoneProvider, Selected?.Id);
		return Details.RefreshAsync();
	}

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		var productRows = (await _gnomeshadeClient.GetProductRowsAsync()).ToList();

		var sortDescriptions = DataGridView.SortDescriptions;
		var selected = Selected;
		Rows = new(productRows);
		DataGridView.SortDescriptions.AddRange(sortDescriptions);
		DataGridView.Filter = Filter.Filter;
		Selected = Rows.SingleOrDefault(overview => overview.Id == selected?.Id);
		if (Selected is null)
		{
			await Details.RefreshAsync();
		}

		Filter.Units = await _gnomeshadeClient.GetUnitsAsync();
		Filter.Categories = await _gnomeshadeClient.GetCategoriesAsync();

		foreach (var row in Rows.ToList())
		{
			var purchases = await _gnomeshadeClient.GetProductPurchasesAsync(row.Id);
			row.PurchaseCount = purchases.Count;
		}
	}

	/// <inheritdoc />
	protected override async Task DeleteAsync(ProductRow row)
	{
		await _gnomeshadeClient.DeleteProductAsync(row.Id);
		Details = new(ActivityService, _gnomeshadeClient, _dateTimeZoneProvider, null);
	}

	private async void OnProductUpserted(object? sender, UpsertedEventArgs e)
	{
		await RefreshAsync();
	}

	private void FilterOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is not (nameof(ProductFilter.Units) or nameof(ProductFilter.Categories)))
		{
			DataGridView.Refresh();
		}
	}
}
