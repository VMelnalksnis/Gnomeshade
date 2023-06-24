// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;

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
	private List<CandlesticksSeries<FinancialPoint>> _series;

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
		_series = new();

		// If this is not initialized, the chart will throw due to index out of bounds
		YAxes = new() { new Axis() };
		XAxes = new() { DateAxis.GetXAxis() };
	}

	/// <summary>Gets the x axes for <see cref="Series"/>.</summary>
	public List<ICartesianAxis> XAxes { get; }

	/// <summary>Gets the y axes for <see cref="Series"/>.</summary>
	public List<ICartesianAxis> YAxes { get; }

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		var counterparty = await _gnomeshadeClient.GetMyCounterpartyAsync();
		var allAccounts = await _gnomeshadeClient.GetAccountsAsync();
		var accounts = allAccounts.Where(account => account.CounterpartyId == counterparty.Id);
		var inCurrencyIds = accounts.SelectMany(account => account.Currencies.Select(aic => aic.Id)).ToList();

		var allTransactions =
			await _gnomeshadeClient.GetDetailedTransactionsAsync(new(Instant.MinValue, Instant.MaxValue));
		var transactions = allTransactions
			.Where(transaction => transaction.Transfers.Any(transfer =>
				inCurrencyIds.Contains(transfer.SourceAccountId) ||
				inCurrencyIds.Contains(transfer.TargetAccountId)))
			.OrderBy(transaction => transaction.ValuedAt ?? transaction.BookedAt)
			.ThenBy(transaction => transaction.CreatedAt)
			.ThenBy(transaction => transaction.ModifiedAt)
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
				.ToList();

			var sumBefore = transactionsBefore.Sum(transaction => transaction.TransferBalance);
			var sumAfter = sumBefore + transactionsIn.Sum(transaction => transaction.TransferBalance);
			var sums = transactionsIn
				.Select((_, index) =>
					sumBefore + transactionsIn.Where((_, i) => i <= index).Sum(t => t.TransferBalance))
				.ToList();

			return new FinancialPoint(
				split.ToDateTimeUnspecified(),
				(double?)sums.Max(),
				(double?)sumBefore,
				(double?)sumAfter,
				(double?)sums.Min());
		});

		Series = new() { new() { Values = values.ToList(), Name = counterparty.Name } };
	}
}
