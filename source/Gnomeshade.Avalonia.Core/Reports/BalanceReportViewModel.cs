﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Accounts;

using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;

using NodaTime;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Reports;

/// <summary>Overview of account balance over time.</summary>
public sealed partial class BalanceReportViewModel : ViewModelBase
{
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly IClock _clock;
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider;

	/// <summary>Gets the data series of balance of the users account over time.</summary>
	[Notify(Setter.Private)]
	private List<CandlesticksSeries<FinancialPoint>> _series = [];

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

	/// <summary>Initializes a new instance of the <see cref="BalanceReportViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">A strongly typed API client.</param>
	/// <param name="clock">Clock which can provide the current instant.</param>
	/// <param name="dateTimeZoneProvider">Time zone provider for localizing instants to local time.</param>
	public BalanceReportViewModel(
		IActivityService activityService,
		IGnomeshadeClient gnomeshadeClient,
		IClock clock,
		IDateTimeZoneProvider dateTimeZoneProvider)
		: base(activityService)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_clock = clock;
		_dateTimeZoneProvider = dateTimeZoneProvider;

		// If this is not initialized, the chart will throw due to index out of bounds
		YAxes = [new Axis()];
		XAxes = [DateAxis.GetXAxis()];

		PropertyChanging += OnPropertyChanging;
		PropertyChanged += OnPropertyChanged;
		_selectedAccounts.CollectionChanged += SelectedAccountsOnCollectionChanged;
	}

	/// <summary>Gets the x axes for <see cref="Series"/>.</summary>
	public List<ICartesianAxis> XAxes { get; }

	/// <summary>Gets the y axes for <see cref="Series"/>.</summary>
	public List<ICartesianAxis> YAxes { get; }

	/// <summary>Resets the zoom for all <see cref="XAxes"/> and <see cref="YAxes"/>.</summary>
	public void ResetZoom()
	{
		foreach (var axis in XAxes.Concat(YAxes))
		{
			axis.MinLimit = null;
			axis.MaxLimit = null;
		}
	}

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		var allTransactionsTask = _gnomeshadeClient.GetDetailedTransactionsAsync(new(Instant.MinValue, Instant.MaxValue));
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

		IEnumerable<Account> accounts = SelectedAccounts.Count is not 0 ? SelectedAccounts : UserAccounts;

		var inCurrencyIds = accounts
			.SelectMany(account => account.Currencies.Where(aic => aic.CurrencyId == (SelectedCurrency?.Id ?? account.PreferredCurrencyId)).Select(aic => aic.Id))
			.ToArray();

		var transactions = (await allTransactionsTask)
			.Where(transaction => transaction.Transfers.Any(transfer =>
				inCurrencyIds.Contains(transfer.SourceAccountId) ||
				inCurrencyIds.Contains(transfer.TargetAccountId)))
			.OrderBy(transaction => transaction.ValuedAt ?? transaction.BookedAt)
			.ThenBy(transaction => transaction.CreatedAt)
			.ThenBy(transaction => transaction.ModifiedAt)
			.ToArray();

		var timeZone = _dateTimeZoneProvider.GetSystemDefault();
		var currentTime = _clock.GetCurrentInstant();
		var dates = transactions
			.Select(transaction => transaction.ValuedAt ?? transaction.BookedAt!.Value)
			.DefaultIfEmpty(currentTime)
			.ToArray();

		var minDate = new ZonedDateTime(dates.Min(), timeZone);
		var maxDate = new ZonedDateTime(dates.Max(), timeZone);
		var splits = minDate.SplitByMonthUntil(maxDate);

		var values = splits.Select(split =>
		{
			var splitZonedDate = split.AtStartOfDayInZone(timeZone);
			var splitInstant = splitZonedDate.ToInstant();

			var transactionsBefore = transactions
				.Where(transaction =>
					new ZonedDateTime(transaction.ValuedAt ?? transaction.BookedAt!.Value, timeZone).ToInstant() < splitInstant);

			var transactionsIn = transactions
				.Where(transaction =>
					new ZonedDateTime(transaction.ValuedAt ?? transaction.BookedAt!.Value, timeZone).Year == splitZonedDate.Year &&
					new ZonedDateTime(transaction.ValuedAt ?? transaction.BookedAt!.Value, timeZone).Month == splitZonedDate.Month)
				.ToArray();

			var sumBefore = transactionsBefore.SumForAccounts(inCurrencyIds);

			var sumAfter = sumBefore + transactionsIn.SumForAccounts(inCurrencyIds);
			var sums = transactionsIn
				.Select((_, index) => sumBefore + transactionsIn.Where((_, i) => i <= index).SumForAccounts(inCurrencyIds))
				.ToArray();

			return new FinancialPoint(
				split.ToDateTimeUnspecified(),
				(double?)sums.Concat(new[] { sumBefore, sumAfter }).Max(),
				(double?)sumBefore,
				(double?)sumAfter,
				(double?)sums.Append(sumBefore).Min());
		});

		Series = [new() { Values = values.ToArray(), Name = counterparty.Name }];
	}

	private void OnPropertyChanging(object? sender, PropertyChangingEventArgs e)
	{
		if (e.PropertyName is nameof(SelectedAccounts))
		{
			SelectedAccounts.CollectionChanged -= SelectedAccountsOnCollectionChanged;
		}
	}

	private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(SelectedAccounts))
		{
			SelectedAccounts.CollectionChanged += SelectedAccountsOnCollectionChanged;
		}

		if (!IsBusy && e.PropertyName is nameof(SelectedCurrency))
		{
			await RefreshAsync();
		}
	}

	private async void SelectedAccountsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (!IsBusy)
		{
			await RefreshAsync();
		}
	}
}
