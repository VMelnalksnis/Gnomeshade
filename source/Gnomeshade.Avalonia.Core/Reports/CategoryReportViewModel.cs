// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;

using Gnomeshade.Avalonia.Core.Products;
using Gnomeshade.Avalonia.Core.Reports.Splits;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Products;
using Gnomeshade.WebApi.Models.Transactions;

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
	private List<Category> _categories = [];

	/// <summary>Gets or sets the category fort which to display the category breakdown for.</summary>
	[Notify]
	private Category? _selectedCategory;

	/// <summary>Gets or sets the selected split from <see cref="Splits"/>.</summary>
	[Notify]
	private IReportSplit? _selectedSplit = SplitProvider.MonthlySplit;

	/// <summary>Gets the data series of amount spent per month per category.</summary>
	[Notify(Setter.Private)]
	private List<StackedColumnSeries<IndexedPoint>> _series = [];

	/// <summary>Gets the x axes for <see cref="Series"/>.</summary>
	[Notify(Setter.Private)]
	private List<ICartesianAxis> _xAxes = [new Axis()];

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

		PropertyChanged += OnPropertyChanged;
	}

	/// <inheritdoc cref="AutoCompleteSelectors.Category"/>
	public AutoCompleteSelector<object> CategorySelector => AutoCompleteSelectors.Category;

	/// <summary>Gets a collection of all available periods.</summary>
	public IEnumerable<IReportSplit> Splits => SplitProvider.Splits;

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		Categories = await _gnomeshadeClient.GetCategoriesAsync();
		if (SelectedSplit is not { } reportSplit)
		{
			return;
		}

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

		var startTime = new ZonedDateTime(dates.Min(), timeZone);
		var endTime = new ZonedDateTime(dates.Max(), timeZone);
		var splits = reportSplit.GetSplits(startTime, endTime).ToArray();

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

		var series = new List<StackedColumnSeries<IndexedPoint>>(groupings.Length);
		for (var seriesIndex = 0; seriesIndex < groupings.Length; seriesIndex++)
		{
			var values = new IndexedPoint[splits.Length];
			var categoryGrouping = groupings[seriesIndex];

			for (var valueIndex = 0; valueIndex < splits.Length; valueIndex++)
			{
				var split = splits[valueIndex];
				var zonedSplit = split.AtStartOfDayInZone(timeZone);

				var purchasesToSum = categoryGrouping
					.Where(purchase => reportSplit.Equals(zonedSplit, purchase.Date))
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

				values[valueIndex] = new((double)sum);
			}

			series.Add(new() { Values = values, Name = categoryGrouping.Key?.Name ?? "Uncategorized" });
		}

		Series = series;
		XAxes = [reportSplit.GetXAxis(startTime, endTime)];
	}

	private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(SelectedSplit) && SelectedSplit is not null)
		{
			await RefreshAsync();
		}
	}

	internal readonly struct PurchaseData(Purchase purchase, CategoryNode? node, TransactionData transaction)
	{
		public Purchase Purchase { get; } = purchase;

		public ZonedDateTime Date { get; } = transaction.Date;

		public List<Transfer> Transfers { get; } = transaction.Transaction.Transfers;

		public CategoryNode? Node { get; } = node;

		public Guid[] SourceCurrencyIds { get; } = transaction.SourceCurrencyIds;

		public Guid[] TargetCurrencyIds { get; } = transaction.TargetCurrencyIds;
	}

	internal readonly struct TransactionData(DetailedTransaction transaction, ZonedDateTime date, Guid[] sourceCurrencyIds, Guid[] targetCurrencyIds)
	{
		public DetailedTransaction Transaction { get; } = transaction;

		public ZonedDateTime Date { get; } = date;

		public Guid[] SourceCurrencyIds { get; } = sourceCurrencyIds;

		public Guid[] TargetCurrencyIds { get; } = targetCurrencyIds;
	}
}
