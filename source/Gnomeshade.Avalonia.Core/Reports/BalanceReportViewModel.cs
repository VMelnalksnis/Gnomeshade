﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;

using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;

using NodaTime;

namespace Gnomeshade.Avalonia.Core.Reports;

/// <summary>Overview of account balance over time.</summary>
public sealed class BalanceReportViewModel : ViewModelBase
{
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly IClock _clock;
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider;

	private List<CandlesticksSeries<FinancialPoint>> _series;
	private List<ICartesianAxis> _xAxes;

	/// <summary>Initializes a new instance of the <see cref="BalanceReportViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">A strongly typed API client.</param>
	/// <param name="clock">Clock which can provide the current instant.</param>
	/// <param name="dateTimeZoneProvider">Time zone provider for localizing instants to local time.</param>
	public BalanceReportViewModel(
		IGnomeshadeClient gnomeshadeClient,
		IClock clock,
		IDateTimeZoneProvider dateTimeZoneProvider)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_clock = clock;
		_dateTimeZoneProvider = dateTimeZoneProvider;
		_series = new();
		_xAxes = new();
	}

	/// <summary>Gets the data series of balance of the users account over time.</summary>
	public List<CandlesticksSeries<FinancialPoint>> Series
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

		XAxes = new()
		{
			new Axis
			{
				Labeler = value =>
					new DateTime(Math.Clamp((long)value, DateTime.MinValue.Ticks, DateTime.MaxValue.Ticks)).ToString(
						"yyyy MM"),
				UnitWidth = TimeSpan.FromDays(30.4375).Ticks,
				MinStep = TimeSpan.FromDays(30.4375).Ticks,
			},
		};

		var values = splits.Select(split =>
		{
			var transactionsBefore = transactions
				.Where(transaction =>
					new ZonedDateTime(transaction.ValuedAt ?? transaction.BookedAt!.Value, timeZone).ToInstant() <
					split.AtStartOfDayInZone(timeZone).ToInstant());

			var transactionsIn = transactions
				.Where(transaction =>
					new ZonedDateTime(transaction.ValuedAt ?? transaction.BookedAt!.Value, timeZone).Year ==
					split.AtStartOfDayInZone(timeZone).Year &&
					new ZonedDateTime(transaction.ValuedAt ?? transaction.BookedAt!.Value, timeZone).Month ==
					split.AtStartOfDayInZone(timeZone).Month)
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

		Series = new() { new() { Values = values.ToList() } };
	}
}