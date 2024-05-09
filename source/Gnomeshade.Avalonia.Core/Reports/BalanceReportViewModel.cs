// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Avalonia.Core.Reports.Splits;
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
	private List<CandlesticksSeries<FinancialPointI>> _series = [];

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

	/// <summary>Gets or sets the selected split from <see cref="Splits"/>.</summary>
	[Notify]
	private IReportSplit? _selectedSplit = SplitProvider.MonthlySplit;

	/// <summary>Gets the y axes for <see cref="Series"/>.</summary>
	[Notify(Setter.Private)]
	private List<ICartesianAxis> _yAxes;

	/// <summary>Gets the x axes for <see cref="Series"/>.</summary>
	[Notify(Setter.Private)]
	private List<ICartesianAxis> _xAxes;

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
		_yAxes = [new Axis()];
		_xAxes = [new Axis()];

		PropertyChanging += OnPropertyChanging;
		PropertyChanged += OnPropertyChanged;
		_selectedAccounts.CollectionChanged += SelectedAccountsOnCollectionChanged;
	}

	/// <summary>Gets a collection of all available periods.</summary>
	public IEnumerable<IReportSplit> Splits => SplitProvider.Splits;

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
		var transfersTask = _gnomeshadeClient.GetTransfersAsync();
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

		if (SelectedSplit is not { } reportSplit)
		{
			return;
		}

		IEnumerable<Account> accounts = SelectedAccounts.Count is not 0 ? SelectedAccounts : UserAccounts;

		var inCurrencyIds = accounts
			.SelectMany(account => account.Currencies.Where(aic => aic.CurrencyId == (SelectedCurrency?.Id ?? account.PreferredCurrencyId)).Select(aic => aic.Id))
			.ToArray();

		var transfers = (await transfersTask)
			.Where(transfer =>
				inCurrencyIds.Contains(transfer.SourceAccountId) ||
				inCurrencyIds.Contains(transfer.TargetAccountId))
			.OrderBy(transfer => transfer.ValuedAt ?? transfer.BookedAt)
			.ThenBy(transfer => transfer.CreatedAt)
			.ThenBy(transfer => transfer.ModifiedAt)
			.ToArray();

		var timeZone = _dateTimeZoneProvider.GetSystemDefault();
		var currentTime = _clock.GetCurrentInstant();
		var dates = transfers
			.Select(transfer => transfer.ValuedAt ?? transfer.BookedAt!.Value)
			.DefaultIfEmpty(currentTime)
			.ToArray();

		var startTime = new ZonedDateTime(dates.Min(), timeZone);
		var endTime = new ZonedDateTime(dates.Max(), timeZone);

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
					(double)sums.Concat(new[] { sumBefore, sumAfter }).Max(),
					(double)sumBefore,
					(double)sumAfter,
					(double)sums.Append(sumBefore).Min());
			});

		Series = [new() { Values = values.ToArray(), Name = counterparty.Name }];
		XAxes = [reportSplit.GetXAxis(startTime, endTime)];
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

		if (e.PropertyName is nameof(SelectedSplit) && SelectedSplit is not null)
		{
			await RefreshAsync();
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
