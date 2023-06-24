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
		XAxes = new() { DateAxis.GetXAxis() };
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
		var accountsInCurrency = accounts.SelectMany(account => account.Currencies).ToArray();
		var allTransactions = await _gnomeshadeClient.GetDetailedTransactionsAsync(new(Instant.MinValue, Instant.MaxValue));
		var displayableTransactions = allTransactions
			.Select(transaction => transaction with { TransferBalance = -transaction.TransferBalance })
			.Where(transaction => transaction.TransferBalance > 0)
			.ToArray();

		var timeZone = _dateTimeZoneProvider.GetSystemDefault();

		var transactions = new TransactionData[displayableTransactions.Length];
		for (var i = 0; i < displayableTransactions.Length; i++)
		{
			var transaction = displayableTransactions[i];
			var date = new ZonedDateTime(transaction.ValuedAt ?? transaction.BookedAt!.Value, timeZone);

			var transfers = transaction.Transfers;
			var transferAccounts = transfers
				.Select(transfer => transfer.SourceAccountId)
				.Concat(transfers.Select(transfer => transfer.TargetAccountId))
				.ToArray();

			var sourceCurrencyIds = accountsInCurrency
				.IntersectBy(transferAccounts, currency => currency.Id)
				.Select(currency => currency.CurrencyId)
				.Distinct()
				.ToArray();

			var targetCurrencyIds = accountsInCurrency
				.IntersectBy(transferAccounts, currency => currency.Id)
				.Select(account => account.CurrencyId)
				.Distinct()
				.ToArray();

			transactions[i] = new(transaction, date, sourceCurrencyIds, targetCurrencyIds);
		}

		var currentTime = _clock.GetCurrentInstant();
		var dates = transactions
			.Select(transaction => transaction.Date.ToInstant())
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

		var uncategorizedTransfers = transactions
			.Where(data => data.Transaction.TransferBalance > data.Transaction.PurchaseTotal)
			.Select(data =>
			{
				var purchase = new Purchase { Price = data.Transaction.TransferBalance - data.Transaction.PurchaseTotal };
				return new PurchaseData(purchase, null, data);
			})
			.ToList();

		var groupings = transactions
			.SelectMany(data => data.Transaction.Purchases.Select(purchase =>
			{
				var product = products.SingleOrDefault(product => product.Id == purchase.ProductId);
				var node = product?.CategoryId is not { } id
					? null
					: id == SelectedCategory?.Id
						? nodes.SingleOrDefault(node => node.Id == SelectedCategory?.Id)
						: nodes.SingleOrDefault(node => node.Id != SelectedCategory?.Id && node.Contains(id));

				return new PurchaseData(purchase, node, data);
			}))
			.Concat(uncategorizedTransfers)
			.Where(data => SelectedCategory is null || data.Node is not null)
			.GroupBy(data => data.Node)
			.ToArray();

		var series = new List<StackedColumnSeries<DateTimePoint>>(groupings.Length);
		for (var seriesIndex = 0; seriesIndex < groupings.Length; seriesIndex++)
		{
			var values = new DateTimePoint[splits.Count];
			var categoryGrouping = groupings[seriesIndex];

			for (var valueIndex = 0; valueIndex < splits.Count; valueIndex++)
			{
				var split = splits[valueIndex];
				var date = split.ToDateTimeUnspecified();
				var purchasesToSum = categoryGrouping
					.Where(purchase => purchase.Date.Year == split.Year && purchase.Date.Month == split.Month)
					.ToArray();

				var sum = 0m;
				for (var purchaseIndex = 0; purchaseIndex < purchasesToSum.Length; purchaseIndex++)
				{
					var purchase = purchasesToSum[purchaseIndex];
					var sourceCurrencyIds = purchase.SourceCurrencyIds;
					var targetCurrencyIds = purchase.TargetCurrencyIds;

					if (sourceCurrencyIds.Length is not 1 || targetCurrencyIds.Length is not 1)
					{
						// todo cannot handle multiple currencies (#686)
						sum += purchase.Purchase.Price;
						continue;
					}

					var sourceCurrency = sourceCurrencyIds.Single();
					var targetCurrency = targetCurrencyIds.Single();

					if (sourceCurrency == targetCurrency)
					{
						sum += purchase.Purchase.Price;
						continue;
					}

					var transfer = purchase.Transfers.Single();
					var ratio = transfer.SourceAmount / transfer.TargetAmount;
					sum += Math.Round(purchase.Purchase.Price * ratio, 2);
				}

				values[valueIndex] = new(date, (double)sum);
			}

			series.Add(new() { Values = values, Name = categoryGrouping.Key?.Name ?? "Uncategorized" });
		}

		Series = series;
	}

	private readonly struct PurchaseData
	{
		public PurchaseData(Purchase purchase, CategoryNode? node, TransactionData transaction)
		{
			Purchase = purchase;
			Node = node;
			Date = transaction.Date;
			Transfers = transaction.Transaction.Transfers;
			SourceCurrencyIds = transaction.SourceCurrencyIds;
			TargetCurrencyIds = transaction.TargetCurrencyIds;
		}

		public Purchase Purchase { get; }

		public ZonedDateTime Date { get; }

		public List<Transfer> Transfers { get; }

		public CategoryNode? Node { get; }

		public Guid[] SourceCurrencyIds { get; }

		public Guid[] TargetCurrencyIds { get; }
	}

	private readonly struct TransactionData
	{
		public TransactionData(
			DetailedTransaction transaction,
			ZonedDateTime date,
			Guid[] sourceCurrencyIds,
			Guid[] targetCurrencyIds)
		{
			Transaction = transaction;
			Date = date;
			SourceCurrencyIds = sourceCurrencyIds;
			TargetCurrencyIds = targetCurrencyIds;
		}

		public DetailedTransaction Transaction { get; }

		public ZonedDateTime Date { get; }

		public Guid[] SourceCurrencyIds { get; }

		public Guid[] TargetCurrencyIds { get; }
	}
}
