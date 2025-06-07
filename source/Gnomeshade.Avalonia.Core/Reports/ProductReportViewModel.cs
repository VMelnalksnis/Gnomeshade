// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;

using Gnomeshade.Avalonia.Core.Reports.Aggregates;
using Gnomeshade.Avalonia.Core.Reports.Calculations;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Products;
using Gnomeshade.WebApi.Models.Transactions;

using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;

using NodaTime;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Reports;

/// <summary>Graphical overview of product prices over time.</summary>
public sealed partial class ProductReportViewModel : ViewModelBase
{
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly IClock _clock;
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider;

	/// <summary>Gets all available products for <see cref="SelectedProduct"/>.</summary>
	[Notify(Setter.Private)]
	private List<Product> _products;

	/// <summary>Gets all products displayed in <see cref="Series"/>.</summary>
	[Notify(Setter.Private)]
	private ObservableCollection<Product> _displayedProducts;

	/// <summary>Gets or sets the product for which to display <see cref="Series"/>.</summary>
	[Notify]
	private Product? _selectedProduct;

	/// <summary>Gets or sets the product selected from <see cref="DisplayedProducts"/>.</summary>
	[Notify]
	private Product? _selectedDisplayedProduct;

	/// <summary>Gets the data series of average price of <see cref="SelectedProduct"/>.</summary>
	[Notify(Setter.Private)]
	private ObservableCollection<LineSeries<DateTimePoint>> _series;

	/// <summary>Gets the y axes for <see cref="Series"/>.</summary>
	[Notify(Setter.Private)]
	private List<ICartesianAxis> _yAxes;

	/// <summary>Gets or sets the aggregate function used to summarize values in each period.</summary>
	[Notify]
	private IAggregateFunction? _selectedAggregate;

	/// <summary>Gets or sets the aggregate function used to summarize values in each period.</summary>
	[Notify]
	private ICalculationFunction? _selectedCalculator;

	/// <summary>Initializes a new instance of the <see cref="ProductReportViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">A strongly typed API client.</param>
	/// <param name="clock">Clock which can provide the current instant.</param>
	/// <param name="dateTimeZoneProvider">Time zone provider for localizing instants to local time.</param>
	public ProductReportViewModel(
		IActivityService activityService,
		IGnomeshadeClient gnomeshadeClient,
		IClock clock,
		IDateTimeZoneProvider dateTimeZoneProvider)
		: base(activityService)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_clock = clock;
		_dateTimeZoneProvider = dateTimeZoneProvider;

		Aggregates = [new Average(), new Maximum(), new Minimum(), new Median(), new Sum()];
		Calculators = [new RelativePricePerUnit(), new PricePerUnit(), new TotalPrice(), new RelativeTotalPrice()];

		_selectedAggregate = Aggregates.First();
		_selectedCalculator = Calculators.First();

		_products = [];
		_displayedProducts = [];
		XAxes = [DateAxis.GetXAxis()];
		_yAxes = [new Axis { MinLimit = 0, Labeler = Labeler }];
		_series = [];

		PropertyChanged += OnPropertyChanged;
		DisplayedProducts.CollectionChanged += DisplayedProductsOnCollectionChanged;
	}

	/// <summary>Gets a delegate for formatting an product in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> ProductSelector => AutoCompleteSelectors.Product;

	/// <summary>Gets a delegate for formatting an aggregate function in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> AggregateSelector => AutoCompleteSelectors.Aggregate;

	/// <summary>Gets a delegate for formatting a calculation function in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> CalculatorSelector => AutoCompleteSelectors.Calculation;

	/// <summary>Gets all available aggregate functions.</summary>
	public List<IAggregateFunction> Aggregates { get; }

	/// <summary>Gets all available aggregate functions.</summary>
	public List<ICalculationFunction> Calculators { get; }

	/// <summary>Gets the x axes for <see cref="Series"/>.</summary>
	public List<ICartesianAxis> XAxes { get; }

	/// <summary>Gets a value indicating whether <see cref="SelectedDisplayedProduct"/> can be removed from <see cref="DisplayedProducts"/>.</summary>
	public bool CanRemoveProduct => SelectedDisplayedProduct is not null;

	private Func<double, string> Labeler => SelectedCalculator is RelativeTotalPrice or RelativePricePerUnit
		? value => value.ToString("P0")
		: value => value.ToString("N2");

	/// <summary>Removes <see cref="SelectedDisplayedProduct"/> from <see cref="DisplayedProducts"/>.</summary>
	public void RemoveSelectedProduct()
	{
		if (SelectedDisplayedProduct is not { } product)
		{
			return;
		}

		DisplayedProducts.Remove(product);
		if (Series.SingleOrDefault(columnSeries => columnSeries.Name == product.Name) is { } series)
		{
			Series.Remove(series);
		}
	}

	/// <summary>Adds the <see cref="SelectedProduct"/> to <see cref="Series"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task UpdateSelectedProduct()
	{
		await RefreshAsync(); // Need await so that SelectedProduct updates
		if (SelectedProduct is null || SelectedAggregate is null || SelectedCalculator is null)
		{
			return;
		}

		using var activity = BeginActivity("Updating product");
		var transactionsTask = _gnomeshadeClient.GetDetailedTransactionsAsync(new(Instant.MinValue, Instant.MaxValue));
		var unitsTask = _gnomeshadeClient.GetUnitsAsync();

		await Task.WhenAll(transactionsTask, unitsTask);

		var detailedTransactions = transactionsTask.Result;
		var units = unitsTask.Result;
		YAxes = [new Axis { MinLimit = 0, Labeler = Labeler }];
		AddSeriesForProduct(SelectedProduct, SelectedAggregate, SelectedCalculator, detailedTransactions, units);
		DisplayedProducts.Add(SelectedProduct);
		SelectedProduct = null;
	}

	/// <summary>Updates all <see cref="Series"/> with the current value of <see cref="SelectedAggregate"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task UpdateSelectedAggregate()
	{
		await RefreshAsync(); // Need await so that Aggregate updates
		if (SelectedAggregate is null || SelectedCalculator is null || !Series.Any())
		{
			return;
		}

		using var activity = BeginActivity("Updating aggregate");
		var transactionsTask = _gnomeshadeClient.GetDetailedTransactionsAsync(new(Instant.MinValue, Instant.MaxValue));
		var unitsTask = _gnomeshadeClient.GetUnitsAsync();

		await Task.WhenAll(transactionsTask, unitsTask);
		var detailedTransactions = transactionsTask.Result;
		var units = unitsTask.Result;

		Series = [];
		YAxes = [new Axis { MinLimit = 0, Labeler = Labeler }];
		foreach (var product in DisplayedProducts)
		{
			AddSeriesForProduct(product, SelectedAggregate, SelectedCalculator, detailedTransactions, units);
		}
	}

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		var productsTask = _gnomeshadeClient.GetProductsAsync();
		var purchasesTask = _gnomeshadeClient.GetPurchasesAsync();
		await Task.WhenAll(productsTask, purchasesTask);

		var productCounts = purchasesTask.Result
			.GroupBy(purchase => purchase.ProductId)
			.ToDictionary(grouping => grouping.Key, grouping => grouping.Count());

		Products = productsTask.Result
			.Except(DisplayedProducts)
			.OrderByDescending(product => productCounts.TryGetValue(product.Id, out var count) ? count : 0)
			.ToList();
	}

	private void AddSeriesForProduct(
		Product productToAdd,
		IAggregateFunction aggregate,
		ICalculationFunction calculationFunction,
		List<DetailedTransaction> detailedTransactions,
		List<Unit> units)
	{
		var transactions = detailedTransactions
			.Where(transaction => transaction.Purchases.Any(purchase => purchase.ProductId == productToAdd.Id))
			.ToList();

		var timeZone = _dateTimeZoneProvider.GetSystemDefault();
		var currentTime = _clock.GetCurrentInstant();
		var dates = transactions
			.Select(transaction => transaction.ValuedAt ?? transaction.BookedAt!.Value)
			.DefaultIfEmpty(currentTime)
			.ToList();

		var minDate = new ZonedDateTime(dates.Min(), timeZone);
		var maxDate = new ZonedDateTime(dates.Max(), timeZone);
		var splits = minDate.SplitByMonthUntil(maxDate);

		var values = transactions
			.SelectMany(transaction =>
			{
				var date = new ZonedDateTime(transaction.ValuedAt ?? transaction.BookedAt!.Value, timeZone);
				return transaction.Purchases
					.Where(purchase => purchase.ProductId == productToAdd.Id)
					.Select(purchase =>
					{
						var product = Products.SingleOrDefault(product => product.Id == purchase.ProductId);
						if (product?.UnitId is null)
						{
							return new CalculableValue(purchase, date, 1m);
						}

						_ = units.Single(unit => unit.Id == product.UnitId).GetBaseUnit(units, out var multiplier);
						return new(purchase, date, multiplier);
					});
			}).ToList();

		var dateTimePoints = splits.Select(split => new DateTimePoint(
				split.ToDateTimeUnspecified(),
				(double?)aggregate.Aggregate(values
					.Where(value => value.Date.Year == split.Year && value.Date.Month == split.Month)
					.ToList()
					.DefaultIfEmpty(new(new() { Price = 0, Amount = 1 }, default, 1m))
					.Select(calculationFunction.Calculate))))
			.ToList();

		Series.Add(new()
		{
			Values = calculationFunction.Update(dateTimePoints).ToArray(),
			Stroke = null,
			DataLabelsPaint = new SolidColorPaint(new(255, 255, 255)),
			DataLabelsSize = 12,
			DataLabelsPosition = DataLabelsPosition.Middle,
			GeometrySize = 0,
			GeometryStroke = null,
			GeometryFill = null,
			LineSmoothness = 0,
			Name = productToAdd.Name,
			DataLabelsFormatter = point => point.Model?.Value is { } value ? Labeler(value) : string.Empty,
		});
	}

	private void DisplayedProductsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		OnPropertyChanged(nameof(DisplayedProducts));
	}

	private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(SelectedDisplayedProduct))
		{
			OnPropertyChanged(nameof(CanRemoveProduct));
		}
	}
}
