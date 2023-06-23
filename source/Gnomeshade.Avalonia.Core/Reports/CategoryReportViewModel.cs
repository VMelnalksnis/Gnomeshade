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

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Reports;

/// <summary>Graphical overview of spending in each category.</summary>
public sealed partial class CategoryReportViewModel : ViewModelBase
{
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly IClock _clock;
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider;

	/// <summary>Gets all the available categories.</summary>
	[Notify(Setter.Private)]
	private List<Category> _categories;

	/// <summary>Gets or sets the category fort which to display the category breakdown for.</summary>
	[Notify]
	private Category? _selectedCategory;

	/// <summary>Gets the data series of amount spent per month per category.</summary>
	[Notify(Setter.Private)]
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

	/// <summary>Gets the x axes for <see cref="Series"/>.</summary>
	public List<ICartesianAxis> XAxes { get; }

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		Categories = await _gnomeshadeClient.GetCategoriesAsync();
		var accounts = await _gnomeshadeClient.GetAccountsAsync();
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
		var nodes = categories
			.Where(category => category.CategoryId == SelectedCategory?.Id || category.Id == SelectedCategory?.Id)
			.Select(category => CategoryNode.FromCategory(category, categories))
			.ToList();

		var data = transactions
			.Select(transaction =>
			{
				var date = new ZonedDateTime(transaction.ValuedAt ?? transaction.BookedAt!.Value, timeZone);
				return (transaction, date);
			})
			.ToList();

		var purchases = data
			.SelectMany(tuple => tuple.transaction.Purchases.Select(purchase => (purchase, tuple.transaction.Transfers, tuple.date)))
			.ToList();

		var uncategorizedTransfers = data
			.Where(tuple => tuple.transaction.TransferBalance > tuple.transaction.PurchaseTotal)
			.Select(tuple =>
			{
				var purchase = new Purchase { Price = tuple.transaction.TransferBalance - tuple.transaction.PurchaseTotal };
				return new PurchaseData(purchase, tuple.date, tuple.transaction.Transfers, null);
			})
			.ToList();

		var purchasesWithCategories = purchases
			.Select(purchase =>
			{
				var product = products.SingleOrDefault(product => product.Id == purchase.purchase.ProductId);
				var node = product?.CategoryId is not { } id
					? null
					: id == SelectedCategory?.Id
						? nodes.SingleOrDefault(node => node.Id == SelectedCategory?.Id)
						: nodes.SingleOrDefault(node => node.Id != SelectedCategory?.Id && node.Contains(id));

				return new PurchaseData(purchase.purchase, purchase.date, purchase.Transfers, node);
			})
			.Concat(uncategorizedTransfers)
			.Where(tuple => SelectedCategory is null || tuple.Node is not null)
			.GroupBy(tuple => tuple.Node)
			.Select(categoryGrouping => new StackedColumnSeries<DateTimePoint>
			{
				Values = splits
					.Select(split => new DateTimePoint(
						split.ToDateTimeUnspecified(),
						(double?)categoryGrouping
							.Where(grouping => grouping.Date.Year == split.Year && grouping.Date.Month == split.Month)
							.Sum(grouping =>
							{
								var sourceCurrencyIds = accounts
									.SelectMany(account => account.Currencies)
									.Where(account => grouping.Transfers.Any(transfer => transfer.SourceAccountId == account.Id))
									.Select(account => account.CurrencyId)
									.Distinct()
									.ToList();

								var targetCurrencyIds = accounts
									.SelectMany(account => account.Currencies)
									.Where(account => grouping.Transfers.Any(transfer => transfer.TargetAccountId == account.Id))
									.Select(account => account.CurrencyId)
									.Distinct()
									.ToList();

								if (sourceCurrencyIds.Count is not 1 ||
									targetCurrencyIds.Count is not 1)
								{
									// todo cannot handle multiple currencies (#686)
									return grouping.Purchase.Price;
								}

								var sourceCurrency = sourceCurrencyIds.Single();
								var targetCurrency = targetCurrencyIds.Single();

								if (sourceCurrency == targetCurrency)
								{
									return grouping.Purchase.Price;
								}

								var transfer = grouping.Transfers.Single();
								var ratio = transfer.SourceAmount / transfer.TargetAmount;
								return Math.Round(grouping.Purchase.Price * ratio, 2);
							})))
					.ToArray(),
				Name = categoryGrouping.Key?.Name ?? "Uncategorized",
			})
			.ToList();

		Series = purchasesWithCategories;
	}

	private struct PurchaseData
	{
		public PurchaseData(Purchase purchase, ZonedDateTime date, List<Transfer> transfers, CategoryNode? node)
		{
			Purchase = purchase;
			Date = date;
			Transfers = transfers;
			Node = node;
		}

		public Purchase Purchase { get; }

		public ZonedDateTime Date { get; }

		public List<Transfer> Transfers { get; }

		public CategoryNode? Node { get; }
	}
}
