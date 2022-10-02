// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
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

namespace Gnomeshade.Avalonia.Core.Reports;

/// <summary>Graphical overview of product prices over time.</summary>
public sealed class ProductReportViewModel : ViewModelBase
{
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly IClock _clock;
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider;
	private List<Product> _products;
	private Product? _selectedProduct;
	private List<ColumnSeries<DateTimePoint>> _series;
	private List<ICartesianAxis> _xAxes;
	private List<ICartesianAxis> _yAxes;
	private IAggregateFunction? _selectedAggregate;
	private ICalculationFunction? _calculationFunction;

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

		Aggregates = new() { new Average(), new Maximum(), new Minimum(), new Median(), new Sum() };
		Calculators = new() { new RelativePricePerUnit(), new PricePerUnit(), new TotalPrice(), new RelativeTotalPrice() };

		_selectedAggregate = Aggregates.First();
		_calculationFunction = Calculators.First();

		_products = new();
		_xAxes = new();
		_series = new();
		_yAxes = new();
	}

	/// <summary>Gets a delegate for formatting an product in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> ProductSelector => AutoCompleteSelectors.Product;

	/// <summary>Gets a delegate for formatting an aggregate function in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> AggregateSelector => (_, item) => ((IAggregateFunction)item).Name;

	/// <summary>Gets a delegate for formatting a calculation function in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> CalculatorSelector => (_, item) => ((ICalculationFunction)item).Name;

	/// <summary>Gets all available products for <see cref="SelectedProduct"/>.</summary>
	public List<Product> Products
	{
		get => _products;
		private set => SetAndNotify(ref _products, value);
	}

	/// <summary>Gets or sets the product for which to display <see cref="Series"/>.</summary>
	public Product? SelectedProduct
	{
		get => _selectedProduct;
		set => SetAndNotify(ref _selectedProduct, value);
	}

	/// <summary>Gets all available aggregate functions.</summary>
	public List<IAggregateFunction> Aggregates { get; }

	/// <summary>Gets or sets the aggregate function used to summarize values in each period.</summary>
	public IAggregateFunction? SelectedAggregate
	{
		get => _selectedAggregate;
		set => SetAndNotify(ref _selectedAggregate, value);
	}

	/// <summary>Gets all available aggregate functions.</summary>
	public List<ICalculationFunction> Calculators { get; }

	/// <summary>Gets or sets the aggregate function used to summarize values in each period.</summary>
	public ICalculationFunction? SelectedCalculator
	{
		get => _calculationFunction;
		set => SetAndNotify(ref _calculationFunction, value);
	}

	/// <summary>Gets the data series of average price of <see cref="SelectedProduct"/>.</summary>
	public List<ColumnSeries<DateTimePoint>> Series
	{
		get => _series;
		private set => SetAndNotify(ref _series, value);
	}

	/// <summary>Gets the x axes for <see cref="Series"/>.</summary>
	public List<ICartesianAxis> XAxes
	{
		get => _xAxes;
		private set => SetAndNotify(ref _xAxes, value);
	}

	/// <summary>Gets the y axes for <see cref="Series"/>.</summary>
	public List<ICartesianAxis> YAxes
	{
		get => _yAxes;
		private set => SetAndNotify(ref _yAxes, value);
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

		using var activity = BeginActivity();
		var transactionsTask = _gnomeshadeClient.GetDetailedTransactionsAsync(new(Instant.MinValue, Instant.MaxValue));
		var unitsTask = _gnomeshadeClient.GetUnitsAsync();

		await Task.WhenAll(transactionsTask, unitsTask);

		var detailedTransactions = transactionsTask.Result;
		var units = unitsTask.Result;
		AddSeriesForProduct(SelectedProduct, SelectedAggregate, SelectedCalculator, detailedTransactions, units);
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

		using var activity = BeginActivity();
		var transactionsTask = _gnomeshadeClient.GetDetailedTransactionsAsync(new(Instant.MinValue, Instant.MaxValue));
		var unitsTask = _gnomeshadeClient.GetUnitsAsync();

		var currentProductNames = Series.Select(series => series.Name).ToList();
		var products = Products.Where(product => currentProductNames.Contains(product.Name));

		await Task.WhenAll(transactionsTask, unitsTask);
		var detailedTransactions = transactionsTask.Result;
		var units = unitsTask.Result;

		Series = new();
		foreach (var product in products)
		{
			AddSeriesForProduct(product, SelectedAggregate, SelectedCalculator, detailedTransactions, units);
		}
	}

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		var products = await _gnomeshadeClient.GetProductsAsync();
		Products = products;
	}

	private void AddSeriesForProduct(Product productToAdd, IAggregateFunction aggregate, ICalculationFunction calculationFunction, List<DetailedTransaction> detailedTransactions, List<Unit> units)
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

		XAxes = new()
		{
			new Axis
			{
				Labeler = value => new DateTime((long)value).ToString("yyyy MM"),
				UnitWidth = TimeSpan.FromDays(30.4375).Ticks,
				MinStep = TimeSpan.FromDays(30.4375).Ticks,
				LabelsRotation = 90,
			},
		};

		YAxes = new() { new Axis { MinLimit = 0 } };

		var values = transactions
			.SelectMany(transaction =>
			{
				var date = new ZonedDateTime(transaction.ValuedAt ?? transaction.BookedAt!.Value, timeZone);
				return transaction.Purchases
					.Where(purchase => purchase.ProductId == productToAdd.Id)
					.Select(purchase =>
					{
						var product = Products.Single(product => product.Id == purchase.ProductId);
						if (product.UnitId is null)
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

		Series.Add(
			new()
			{
				Values = calculationFunction.Update(dateTimePoints),
				Stroke = null,
				DataLabelsPaint = new SolidColorPaint(new(255, 255, 255)),
				DataLabelsSize = 12,
				DataLabelsPosition = DataLabelsPosition.Middle,
				Name = productToAdd.Name,
				EasingFunction = null,
				DataLabelsFormatter = point => point.Model?.Value?.ToString("N2") ?? string.Empty,
			});
	}
}
