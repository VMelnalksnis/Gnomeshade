// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;

using Gnomeshade.Avalonia.Core.Products;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Products;
using Gnomeshade.WebApi.Models.Transactions;

using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;

using NodaTime;

namespace Gnomeshade.Avalonia.Core.Reports;

/// <summary>Graphical overview of spending in each category.</summary>
public sealed class CategoryReportViewModel : ViewModelBase
{
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly IClock _clock;
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider;

	private List<Category> _categories;
	private Category? _selectedCategory;
	private List<StackedColumnSeries<DateTimePoint>> _series;

	/// <summary>Initializes a new instance of the <see cref="CategoryReportViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">A strongly typed API client.</param>
	/// <param name="clock">Clock which can provide the current instant.</param>
	/// <param name="dateTimeZoneProvider">Time zone provider for localizing instants to local time.</param>
	public CategoryReportViewModel(
		IActivityService activityService,
		IGnomeshadeClient gnomeshadeClient,
		IClock clock,
		IDateTimeZoneProvider dateTimeZoneProvider)
		: base(activityService)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_dateTimeZoneProvider = dateTimeZoneProvider;
		_clock = clock;

		_categories = new();
		_series = new();
		XAxes = new()
		{
			new Axis
			{
				Labeler = value => new DateTime(Math.Clamp((long)value, DateTime.MinValue.Ticks, DateTime.MaxValue.Ticks)).ToString("yyyy MM"),
				UnitWidth = TimeSpan.FromDays(30.4375).Ticks,
				MinStep = TimeSpan.FromDays(30.4375).Ticks,
			},
		};
	}

	/// <summary>Gets a delegate for formatting an category in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> CategorySelector => AutoCompleteSelectors.Category;

	/// <summary>Gets all the available categories.</summary>
	public List<Category> Categories
	{
		get => _categories;
		private set => SetAndNotify(ref _categories, value);
	}

	/// <summary>Gets or sets the category fort which to display the category breakdown for.</summary>
	public Category? SelectedCategory
	{
		get => _selectedCategory;
		set => SetAndNotify(ref _selectedCategory, value);
	}

	/// <summary>Gets the data series of amount spent per month per category.</summary>
	public List<StackedColumnSeries<DateTimePoint>> Series
	{
		get => _series;
		private set => SetAndNotify(ref _series, value);
	}

	/// <summary>Gets the x axes for <see cref="Series"/>.</summary>
	public List<ICartesianAxis> XAxes { get; }

	/// <summary>Initializes a new instance of the <see cref="CategoryReportViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">A strongly typed API client.</param>
	/// <param name="clock">Clock which can provide the current instant.</param>
	/// <param name="dateTimeZoneProvider">Time zone provider for localizing instants to local time.</param>
	/// <returns>A new instance of the <see cref="CategoryReportViewModel"/> class.</returns>
	public static async Task<CategoryReportViewModel> CreateAsync(
		IActivityService activityService,
		IGnomeshadeClient gnomeshadeClient,
		IClock clock,
		IDateTimeZoneProvider dateTimeZoneProvider)
	{
		var viewModel = new CategoryReportViewModel(activityService, gnomeshadeClient, clock, dateTimeZoneProvider);
		await viewModel.RefreshAsync();
		return viewModel;
	}

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		Categories = await _gnomeshadeClient.GetCategoriesAsync();
		var allTransactions = await _gnomeshadeClient.GetDetailedTransactionsAsync(new(Instant.MinValue, Instant.MaxValue));
		var transactions = allTransactions
			.Select(transaction => transaction with { TransferBalance = -transaction.TransferBalance })
			.Where(transaction => transaction.TransferBalance > 0)
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

		var products = await _gnomeshadeClient.GetProductsAsync();
		var categories = await _gnomeshadeClient.GetCategoriesAsync();
		var nodes = categories.Where(c => c.CategoryId == SelectedCategory?.Id)
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
				return (purchase, tuple.date, node: default(CategoryNode));
			})
			.ToList();

		var purchasesWithCategories = purchases
			.Select(purchase =>
			{
				var product = products.SingleOrDefault(product => product.Id == purchase.purchase.ProductId);
				var node = product?.CategoryId is not { } id ? null : nodes.SingleOrDefault(node => node.Contains(id));
				return (purchase.purchase, purchase.date, node);
			})
			.Concat(uncategorizedTransfers)
			.Where(tuple => SelectedCategory is null || tuple.node is not null)
			.GroupBy(tuple => tuple.node)
			.Select(categoryGrouping => new StackedColumnSeries<DateTimePoint>
			{
				Values = splits
					.Select(split => new DateTimePoint(
						split.ToDateTimeUnspecified(),
						(double?)categoryGrouping
							.Where(grouping => grouping.date.Year == split.Year && grouping.date.Month == split.Month)
							.Sum(grouping => grouping.purchase.Price))),
				Name = categoryGrouping.Key?.Name ?? "Uncategorized",
			})
			.ToList();

		Series = purchasesWithCategories;
	}
}
