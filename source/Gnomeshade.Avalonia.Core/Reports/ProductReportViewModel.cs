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
	private List<StackedColumnSeries<DateTimePoint>> _series;
	private List<ICartesianAxis> _xAxes;
	private List<ICartesianAxis> _yAxes;

	/// <summary>Initializes a new instance of the <see cref="ProductReportViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">A strongly typed API client.</param>
	/// <param name="clock">Clock which can provide the current instant.</param>
	/// <param name="dateTimeZoneProvider">Time zone provider for localizing instants to local time.</param>
	public ProductReportViewModel(
		IGnomeshadeClient gnomeshadeClient,
		IClock clock,
		IDateTimeZoneProvider dateTimeZoneProvider)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_clock = clock;
		_dateTimeZoneProvider = dateTimeZoneProvider;

		_products = new();
		_xAxes = new();
		_series = new();
		_yAxes = new();
	}

	/// <summary>Gets a delegate for formatting an product in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> ProductSelector => AutoCompleteSelectors.Product;

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

	/// <summary>Gets the data series of average price of <see cref="SelectedProduct"/>.</summary>
	public List<StackedColumnSeries<DateTimePoint>> Series
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

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		var products = await _gnomeshadeClient.GetProductsAsync();

		Products = products;
		if (SelectedProduct is null)
		{
			return;
		}

		var detailedTransactions = await _gnomeshadeClient.GetDetailedTransactionsAsync(Instant.MinValue, Instant.MaxValue);
		var transactions = detailedTransactions
			.Where(transaction => transaction.Purchases.Any(purchase => purchase.ProductId == SelectedProduct.Id))
			.ToList();

		var timeZone = _dateTimeZoneProvider.GetSystemDefault();
		var currentTime = _clock.GetCurrentInstant();
		var minDate = new ZonedDateTime(
			transactions.MinOrDefault(transaction => transaction.ValuedAt ?? transaction.BookedAt!.Value, currentTime),
			timeZone);
		var maxDate = new ZonedDateTime(
			transactions.MaxOrDefault(transaction => transaction.ValuedAt ?? transaction.BookedAt!.Value, currentTime),
			timeZone);
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

		var purchases = transactions
			.SelectMany(transaction =>
			{
				var date = new ZonedDateTime(transaction.ValuedAt ?? transaction.BookedAt!.Value, timeZone);
				return transaction.Purchases
					.Where(purchase => purchase.ProductId == SelectedProduct.Id)
					.Select(purchase => (purchase, date));
			}).ToList();

		Series = new()
		{
			new()
			{
				Values = splits.Select(split => new DateTimePoint(
					split.ToDateTimeUnspecified(),
					(double?)purchases
						.Where(purchase => purchase.date.Year == split.Year && purchase.date.Month == split.Month)
						.ToList()
						.AverageOrDefault(purchase => purchase.purchase.Price))),
				Stroke = null,
				DataLabelsPaint = new SolidColorPaint(new(255, 255, 255)),
				DataLabelsSize = 12,
				DataLabelsPosition = DataLabelsPosition.Middle,
				Name = SelectedProduct?.Name,
				EasingFunction = null,
				DataLabelsFormatter = point => point.Model?.Value?.ToString("N2") ?? string.Empty,
			},
		};
	}
}
