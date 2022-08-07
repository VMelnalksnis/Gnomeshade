// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Avalonia.Core.Products;
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

/// <summary>Graphical overview of spending in each category.</summary>
public sealed class CategoryReportViewModel : ViewModelBase
{
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly IClock _clock;
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider;

	private List<StackedColumnSeries<DateTimePoint>> _series;
	private List<ICartesianAxis> _xAxes;
	private int? _currentYear;

	/// <summary>Initializes a new instance of the <see cref="CategoryReportViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">A strongly typed API client.</param>
	/// <param name="clock">Clock which can provide the current instant.</param>
	/// <param name="dateTimeZoneProvider">Time zone provider for localizing instants to local time.</param>
	public CategoryReportViewModel(
		IGnomeshadeClient gnomeshadeClient,
		IClock clock,
		IDateTimeZoneProvider dateTimeZoneProvider)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_dateTimeZoneProvider = dateTimeZoneProvider;
		_clock = clock;
		_series = new();
		_xAxes = new();
	}

	/// <summary>Gets the data series of amount spent per month per category.</summary>
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

	/// <summary>Initializes a new instance of the <see cref="CategoryReportViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">A strongly typed API client.</param>
	/// <param name="clock">Clock which can provide the current instant.</param>
	/// <param name="dateTimeZoneProvider">Time zone provider for localizing instants to local time.</param>
	/// <returns>A new instance of the <see cref="CategoryReportViewModel"/> class.</returns>
	public static async Task<CategoryReportViewModel> CreateAsync(
		IGnomeshadeClient gnomeshadeClient,
		IClock clock,
		IDateTimeZoneProvider dateTimeZoneProvider)
	{
		var viewModel = new CategoryReportViewModel(gnomeshadeClient, clock, dateTimeZoneProvider);
		await viewModel.RefreshAsync().ConfigureAwait(false);
		return viewModel;
	}

	/// <summary>Set limit for first axis from <see cref="XAxes"/> to the next year.</summary>
	public void NextYear()
	{
		var maxYear = Series.SelectMany(series => series.Values ?? Array.Empty<DateTimePoint>())
			.Max(values => values.DateTime).Year;
		_currentYear ??= maxYear;
		if (_currentYear != maxYear)
		{
			_currentYear++;
		}

		XAxes[0].MinLimit = new DateTime(_currentYear!.Value, 1, 1).AddDays(-15).Ticks;
		XAxes[0].MaxLimit = new DateTime(_currentYear.Value, 12, 31).AddDays(-15).Ticks;
	}

	/// <summary>Set limit for first axis from <see cref="XAxes"/> to the previous year.</summary>
	public void PreviousYear()
	{
		var minYear = Series.SelectMany(series => series.Values ?? Array.Empty<DateTimePoint>())
			.Min(values => values.DateTime).Year;
		_currentYear ??= minYear;
		if (_currentYear != minYear)
		{
			_currentYear--;
		}

		XAxes[0].MinLimit = new DateTime(_currentYear!.Value, 1, 1).AddDays(-15).Ticks;
		XAxes[0].MaxLimit = new DateTime(_currentYear.Value, 12, 31).AddDays(-15).Ticks;
	}

	/// <summary>Clear limits for first axis from <see cref="XAxes"/>.</summary>
	public void Clear()
	{
		XAxes[0].MinLimit = null;
		XAxes[0].MaxLimit = null;
	}

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		var allTransactions = await _gnomeshadeClient.GetDetailedTransactionsAsync(Instant.MinValue, Instant.MaxValue).ConfigureAwait(false);
		var transactions = allTransactions
			.Select(transaction => transaction with { TransferBalance = -transaction.TransferBalance })
			.Where(transaction => transaction.TransferBalance > 0)
			.ToList();

		var timeZone = _dateTimeZoneProvider.GetSystemDefault();
		var currentTime = _clock.GetCurrentInstant();
		var minDate =
			new ZonedDateTime(
				transactions.MinOrDefault(transaction => transaction.ValuedAt ?? transaction.BookedAt!.Value, currentTime),
				timeZone);
		var maxDate =
			new ZonedDateTime(
				transactions.MaxOrDefault(transaction => transaction.ValuedAt ?? transaction.BookedAt!.Value, currentTime),
				timeZone);
		var splits = Split(minDate, maxDate);

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

		var products = await _gnomeshadeClient.GetProductsAsync().ConfigureAwait(false);
		var categories = await _gnomeshadeClient.GetCategoriesAsync().ConfigureAwait(false);
		var nodes = categories.Where(c => c.CategoryId is null)
			.Select(category => CategoryNode.FromCategory(category, categories)).ToList();

		var data = transactions
			.Select(transaction =>
			{
				var date = new ZonedDateTime(transaction.ValuedAt ?? transaction.BookedAt!.Value, timeZone);
				return (transaction, date);
			})
			.ToList();

		var purchases = data
			.SelectMany(tuple => tuple.transaction.Purchases.Select(purchase => (purchase, tuple.date)))
			.ToList();

		var uncategorizedTransfers = data
			.Where(tuple => tuple.transaction.TransferBalance > tuple.transaction.PurchaseTotal)
			.Select(tuple =>
			{
				var purchase = new Purchase { Price = tuple.transaction.TransferBalance - tuple.transaction.PurchaseTotal };
				return (purchase, tuple.date, category: default(Category));
			})
			.ToList();

		var purchasesWithCategories = purchases
			.Select(purchase =>
			{
				var product = products.Single(product => product.Id == purchase.purchase.ProductId);
				var category = product.CategoryId is null
					? null
					: categories.Single(category => category.Id == product.CategoryId);

				var node = category is null ? null : nodes.Single(node => node.Contains(category.Id));
				category = node is null ? category : categories.Single(c => c.Id == node.Id);
				return (purchase.purchase, purchase.date, category);
			})
			.Concat(uncategorizedTransfers)
			.GroupBy(tuple => tuple.category)
			.Select(categoryGrouping => new StackedColumnSeries<DateTimePoint>
			{
				Values = splits
					.Select(split => new DateTimePoint(
						split.ToDateTimeUnspecified(),
						(double?)categoryGrouping
							.Where(grouping => grouping.date.Year == split.Year && grouping.date.Month == split.Month)
							.Sum(grouping => grouping.purchase.Price))),
				Stroke = null,
				DataLabelsPaint = new SolidColorPaint(new(255, 255, 255)),
				DataLabelsSize = 12,
				DataLabelsPosition = DataLabelsPosition.Middle,
				Name = categoryGrouping.Key?.Name ?? "Uncategorized",
				EasingFunction = null,
			})
			.ToList();

		Series = purchasesWithCategories;
	}

	private static List<LocalDate> Split(ZonedDateTime from, ZonedDateTime to)
	{
		var currentDate = new LocalDate(from.Year, from.Month, 1);
		var dates = new List<LocalDate>();
		while (currentDate.Year < to.Year || (currentDate.Year == to.Year && currentDate.Month <= to.Month))
		{
			dates.Add(currentDate);
			currentDate += Period.FromMonths(1);
		}

		return dates;
	}
}
