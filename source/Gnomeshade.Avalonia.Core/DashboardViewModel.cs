// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Avalonia.Core.Products;
using Gnomeshade.Avalonia.Core.Reports;
using Gnomeshade.Avalonia.Core.Reports.Splits;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Accounts;
using Gnomeshade.WebApi.Models.Transactions;

using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;

using NodaTime;

using PropertyChanged.SourceGenerator;

using SkiaSharp;

namespace Gnomeshade.Avalonia.Core;

/// <summary>Quick overview of all accounts.</summary>
public sealed partial class DashboardViewModel : ViewModelBase
{
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly IClock _clock;
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider;

	/// <summary>Gets the data series of the balance of all user accounts with positive balance.</summary>
	[Notify(Setter.Private)]
	private PieSeries<decimal>[] _balanceSeries = [];

	/// <summary>Gets the data series of the balance of all user accounts with negative balance.</summary>
	[Notify(Setter.Private)]
	private PieSeries<decimal>[] _liabilitiesBalanceSeries = [];

	/// <summary>Gets the data series of spending by category.</summary>
	[Notify(Setter.Private)]
	private PieSeries<decimal>[] _categoriesSeries = [];

	/// <summary>Gets the data series of balance of the users account over time.</summary>
	[Notify(Setter.Private)]
	private CandlesticksSeries<FinancialPointI>[] _cashflowSeries = [];

	/// <summary>Gets a collection of all accounts of the current user.</summary>
	[Notify(Setter.Private)]
	private List<Account> _userAccounts = [];

	/// <summary>Gets or sets a collection of accounts selected from <see cref="UserAccounts"/>.</summary>
	[Notify]
	private ObservableCollection<Account> _selectedAccounts = [];

	/// <summary>Gets a collection of all currencies used in <see cref="UserAccounts"/>.</summary>
	[Notify(Setter.Private)]
	private List<Currency> _currencies = [];

	/// <summary>Gets or sets the selected currency from <see cref="Currencies"/>.</summary>
	[Notify]
	private Currency? _selectedCurrency;

	/// <summary>Gets the y axes for <see cref="CashflowSeries"/>.</summary>
	[Notify(Setter.Private)]
	private ICartesianAxis[] _yAxes = [new Axis()];

	/// <summary>Gets the x axes for <see cref="CashflowSeries"/>.</summary>
	[Notify(Setter.Private)]
	private ICartesianAxis[] _xAxes = [new Axis()];

	/// <summary>Gets the collection of account summary rows.</summary>
	[Notify(Setter.Private)]
	private AccountRow[] _accountRows = [];

	/// <summary>Gets the collection of rows of various totals of <see cref="AccountRows"/>.</summary>
	[Notify(Setter.Private)]
	private AccountRow[] _accountRowsTotals = [];

	/// <summary>Initializes a new instance of the <see cref="DashboardViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">A strongly typed API client.</param>
	/// <param name="clock">Clock which can provide the current instant.</param>
	/// <param name="dateTimeZoneProvider">Time zone provider for localizing instants to local time.</param>
	public DashboardViewModel(
		IActivityService activityService,
		IGnomeshadeClient gnomeshadeClient,
		IClock clock,
		IDateTimeZoneProvider dateTimeZoneProvider)
		: base(activityService)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_clock = clock;
		_dateTimeZoneProvider = dateTimeZoneProvider;
	}

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		var (counterparty, allAccounts, currencies) = await
			(_gnomeshadeClient.GetMyCounterpartyAsync(),
			_gnomeshadeClient.GetAccountsAsync(),
			_gnomeshadeClient.GetCurrenciesAsync())
			.WhenAll();

		var selected = SelectedAccounts.Select(account => account.Id).ToArray();
		var selectedCurrency = SelectedCurrency?.Id;

		UserAccounts = allAccounts.Where(account => account.CounterpartyId == counterparty.Id).ToList();
		Currencies = currencies
			.Where(currency => UserAccounts.SelectMany(account => account.Currencies).Any(aic => aic.CurrencyId == currency.Id))
			.ToList();

		SelectedAccounts = new(UserAccounts.Where(account => selected.Contains(account.Id)));
		SelectedCurrency = selectedCurrency is { } id ? Currencies.Single(currency => currency.Id == id) : null;

		IReadOnlyCollection<Account> accounts = SelectedAccounts.Count is not 0 ? SelectedAccounts : UserAccounts;

		var timeZone = _dateTimeZoneProvider.GetSystemDefault();
		var currentInstant = _clock.GetCurrentInstant();
		var localTime = currentInstant.InZone(timeZone).LocalDateTime;

		var startTime = localTime.With(DateAdjusters.StartOfMonth).InZoneStrictly(timeZone);
		var endTime = localTime.With(DateAdjusters.EndOfMonth).InZoneStrictly(timeZone);

		await Task.WhenAll(
			RefreshCashFlow(accounts, startTime, endTime, timeZone, counterparty),
			RefreshBalance(),
			RefreshCategories(accounts, timeZone, startTime));
	}

	private async Task RefreshCashFlow(
		IReadOnlyCollection<Account> accounts,
		ZonedDateTime startTime,
		ZonedDateTime endTime,
		DateTimeZone timeZone,
		Counterparty counterparty)
	{
		var inCurrencyIds = accounts
			.SelectMany(account => account.Currencies.Where(aic => aic.CurrencyId == (SelectedCurrency?.Id ?? account.PreferredCurrencyId)).Select(aic => aic.Id))
			.ToArray();

		var transfers = (await _gnomeshadeClient.GetTransfersAsync())
			.Where(transfer =>
				inCurrencyIds.Contains(transfer.SourceAccountId) ||
				inCurrencyIds.Contains(transfer.TargetAccountId))
			.OrderBy(transfer => transfer.ValuedAt ?? transfer.BookedAt)
			.ThenBy(transfer => transfer.CreatedAt)
			.ThenBy(transfer => transfer.ModifiedAt)
			.ToArray();
		var reportSplit = SplitProvider.DailySplit;

		var values = reportSplit
			.GetSplits(startTime, endTime)
			.Select(date =>
			{
				var splitZonedDate = date.AtStartOfDayInZone(timeZone);
				var splitInstant = splitZonedDate.ToInstant();

				var transfersBefore = transfers
					.Where(transfer => new ZonedDateTime(transfer.ValuedAt ?? transfer.BookedAt!.Value, timeZone).ToInstant() < splitInstant);

				var transfersIn = transfers
					.Where(transfer => reportSplit.Equals(
						splitZonedDate,
						new(transfer.ValuedAt ?? transfer.BookedAt!.Value, timeZone)))
					.ToArray();

				var sumBefore = transfersBefore.SumForAccounts(inCurrencyIds);

				var sumAfter = sumBefore + transfersIn.SumForAccounts(inCurrencyIds);
				var sums = transfersIn
					.Select((_, index) => sumBefore + transfersIn.Where((_, i) => i <= index).SumForAccounts(inCurrencyIds))
					.ToArray();

				return new FinancialPointI(
					(double)sums.Concat([sumBefore, sumAfter]).Max(),
					(double)sumBefore,
					(double)sumAfter,
					(double)sums.Append(sumBefore).Min());
			});

		CashflowSeries = [new() { Values = values.ToArray(), Name = counterparty.Name }];
		XAxes = [reportSplit.GetXAxis(startTime, endTime)];
	}

	private async Task RefreshBalance()
	{
		var accountRowTasks = UserAccounts
			.Select(async account =>
			{
				var balances = await _gnomeshadeClient.GetAccountBalanceAsync(account.Id);
				var preferredAccountInCurrency = account
					.Currencies
					.Single(currency => currency.CurrencyId == account.PreferredCurrencyId);

				var balance =
					balances.Single(balance => balance.AccountInCurrencyId == preferredAccountInCurrency.Id);

				return new AccountRow(account.Name, balance.TargetAmount - balance.SourceAmount);
			});

		var rows = await Task.WhenAll(accountRowTasks);
		AccountRows = rows.OrderByDescending(row => row.Balance).ToArray();
		AccountRowsTotals =
		[
			new("Balance", AccountRows.Where(row => row.Balance > 0).Sum(row => row.Balance)),
			new("Liabilities", AccountRows.Where(row => row.Balance < 0).Sum(row => row.Balance)),
			new("Total", AccountRows.Sum(row => row.Balance)),
		];

		var balanceSeries = AccountRows
			.Select(row => new PieSeries<decimal>
			{
				Name = row.Name,
				Values = [row.Balance],
				DataLabelsPaint = new SolidColorPaint(SKColors.Black),
				DataLabelsFormatter = point => point.Model.ToString("N2"),
			})
			.ToArray();

		BalanceSeries = balanceSeries.Where(series => series.Values?.Sum() > 0).ToArray();
		LiabilitiesBalanceSeries = balanceSeries
			.Where(series => series.Values?.Sum() < 0)
			.Select(series =>
			{
				series.Values = series.Values?.Select(x => -x) ?? [];
				return series;
			})
			.ToArray();
	}

	private async Task RefreshCategories(IReadOnlyCollection<Account> accounts, DateTimeZone timeZone, ZonedDateTime startTime)
	{
		var accountsInCurrency = accounts.SelectMany(account => account.Currencies).ToArray();
		var (allTransactions, categories, products) = await (
			_gnomeshadeClient.GetDetailedTransactionsAsync(new(Instant.MinValue, Instant.MaxValue)),
			_gnomeshadeClient.GetCategoriesAsync(),
			_gnomeshadeClient.GetProductsAsync())
			.WhenAll();
		var displayableTransactions = allTransactions
			.Select(transaction => transaction with { TransferBalance = -transaction.TransferBalance })
			.Where(transaction => transaction.TransferBalance > 0)
			.ToArray();

		var transactions = new CategoryReportViewModel.TransactionData[displayableTransactions.Length];
		for (var i = 0; i < displayableTransactions.Length; i++)
		{
			var transaction = displayableTransactions[i];
			var date = new ZonedDateTime(transaction.ValuedAt ?? transaction.BookedAt!.Value, timeZone);

			var transactionTransfers = transaction.Transfers;
			var transferAccounts = transactionTransfers
				.Select(transfer => transfer.SourceAccountId)
				.Concat(transactionTransfers.Select(transfer => transfer.TargetAccountId))
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

		var nodes = categories
			.Where(category => category.CategoryId == null)
			.Select(category => CategoryNode.FromCategory(category, categories))
			.ToList();

		var uncategorizedTransfers = transactions
			.Where(data => data.Transaction.TransferBalance > data.Transaction.PurchaseTotal)
			.Select(data =>
			{
				var purchase = new Purchase { Price = data.Transaction.TransferBalance - data.Transaction.PurchaseTotal };
				return new CategoryReportViewModel.PurchaseData(purchase, null, data);
			})
			.ToList();

		var groupings = transactions
			.SelectMany(data => data.Transaction.Purchases.Select(purchase =>
			{
				var product = products.SingleOrDefault(product => product.Id == purchase.ProductId);
				var node = product?.CategoryId is not { } categoryId
					? null
					: nodes.SingleOrDefault(node => node.Contains(categoryId));

				return new CategoryReportViewModel.PurchaseData(purchase, node, data);
			}))
			.Concat(uncategorizedTransfers)
			.GroupBy(data => data.Node)
			.ToArray();

		CategoriesSeries = groupings
			.Select(grouping =>
			{
				var zonedSplit = startTime;

				var purchasesToSum = grouping
					.Where(purchase => SplitProvider.MonthlySplit.Equals(zonedSplit, purchase.Date))
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

				return new PieSeries<decimal>
				{
					Name = grouping.Key?.Name ?? "Uncategorized",
					Values = [sum],
					DataLabelsPaint = new SolidColorPaint(SKColors.Black),
					DataLabelsFormatter = point => point.Model.ToString("N2"),
				};
			})
			.Where(series => series.Values?.Sum() > 0)
			.OrderByDescending(series => series.Values?.Sum())
			.ToArray();
	}

	/// <summary>Minimal overview of a single <see cref="Account"/>.</summary>
	public sealed class AccountRow : PropertyChangedBase
	{
		/// <summary>Initializes a new instance of the <see cref="AccountRow"/> class.</summary>
		/// <param name="name">The name of the account.</param>
		/// <param name="balance">The balance of the account in the preferred currency.</param>
		public AccountRow(string name, decimal balance)
		{
			Name = name;
			Balance = balance;
		}

		/// <summary>Gets the name of the account.</summary>
		public string Name { get; }

		/// <summary>Gets the account balance in the preferred currency.</summary>
		public decimal Balance { get; }
	}
}
