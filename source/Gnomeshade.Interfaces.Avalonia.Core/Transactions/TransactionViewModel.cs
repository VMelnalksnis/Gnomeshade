// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Collections;

using Gnomeshade.Interfaces.Avalonia.Core.Transactions.Controls;
using Gnomeshade.Interfaces.Avalonia.Core.Transactions.Transfers;
using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;

using NodaTime;

namespace Gnomeshade.Interfaces.Avalonia.Core.Transactions;

/// <summary>Overview of all <see cref="Transaction"/>s.</summary>
public sealed class TransactionViewModel : OverviewViewModel<TransactionOverview, TransactionUpsertionViewModel>
{
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider;

	private TransactionUpsertionViewModel _details;

	private TransactionViewModel(
		IGnomeshadeClient gnomeshadeClient,
		IClock clock,
		IDateTimeZoneProvider dateTimeZoneProvider,
		TransactionUpsertionViewModel details)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_dateTimeZoneProvider = dateTimeZoneProvider;
		_details = details;

		_details.Upserted += DetailsOnUpserted;
		PropertyChanged += OnPropertyChanged;

		var toDate = clock.GetCurrentInstant().InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset();
		Filter = new()
		{
			ToDate = toDate,
			FromDate = new DateTimeOffset(toDate.Year, toDate.Month, 1, 0, 0, 0, toDate.Offset) - TimeSpan.FromDays(1),
		};
		Filter.PropertyChanged += FilterOnPropertyChanged;
		DataGridView.Filter = Filter.Filter;
		DataGridView.SortDescriptions.Add(
			new DataGridComparerSortDesctiption(
				new TransactionOverviewComparer(overview => overview?.BookedAt),
				ListSortDirection.Descending));
		DataGridView.SortDescriptions.Add(
			new DataGridComparerSortDesctiption(
				new TransactionOverviewComparer(overview => overview?.ValuedAt),
				ListSortDirection.Descending));
		DataGridView.SortDescriptions.Add(
			new DataGridComparerSortDesctiption(
				new TransactionOverviewComparer(overview => overview?.ReconciledAt),
				ListSortDirection.Descending));

		Summary = new();
	}

	/// <summary>Gets the transaction filter.</summary>
	public TransactionFilter Filter { get; }

	/// <summary>Gets the summary of displayed transactions.</summary>
	public TransactionSummary Summary { get; }

	/// <summary>Gets a value indicating whether transactions can be refreshed.</summary>
	public bool CanRefresh => Filter.IsValid;

	/// <inheritdoc />
	public override TransactionUpsertionViewModel Details
	{
		get => _details;
		set
		{
			_details.Upserted -= DetailsOnUpserted;
			SetAndNotify(ref _details, value);
			_details.Upserted += DetailsOnUpserted;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="TransactionViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">A strongly typed API client.</param>
	/// <param name="clock">Clock which can provide the current instant.</param>
	/// <param name="dateTimeZoneProvider">Time zone provider for localizing instants to local time.</param>
	/// <returns>A new instance of the <see cref="TransactionViewModel"/> class.</returns>
	public static async Task<TransactionViewModel> CreateAsync(
		IGnomeshadeClient gnomeshadeClient,
		IClock clock,
		IDateTimeZoneProvider dateTimeZoneProvider)
	{
		var upsertionViewModel = await TransactionUpsertionViewModel.CreateAsync(gnomeshadeClient, dateTimeZoneProvider)
			.ConfigureAwait(false);
		var viewModel = new TransactionViewModel(gnomeshadeClient, clock, dateTimeZoneProvider, upsertionViewModel);
		await viewModel.RefreshAsync().ConfigureAwait(false);
		return viewModel;
	}

	/// <inheritdoc />
	public override async Task RefreshAsync()
	{
		var transactionsTask = _gnomeshadeClient.GetTransactionsAsync(
			Filter.FromDate is null
				? null
				: new ZonedDateTime(
					Instant.FromUtc(
						Filter.FromDate.Value.Year,
						Filter.FromDate.Value.Month,
						Filter.FromDate.Value.Day,
						0,
						0),
					_dateTimeZoneProvider.GetSystemDefault()).ToInstant(),
			Filter.ToDate is null
				? null
				: new ZonedDateTime(
					Instant.FromUtc(
						Filter.ToDate.Value.Year,
						Filter.ToDate.Value.Month,
						Filter.ToDate.Value.Day,
						0,
						0),
					_dateTimeZoneProvider.GetSystemDefault()).ToInstant());
		var accountsTask = _gnomeshadeClient.GetAccountsAsync();
		var counterpartiesTask = _gnomeshadeClient.GetCounterpartiesAsync();

		await Task.WhenAll(transactionsTask, accountsTask, counterpartiesTask).ConfigureAwait(false);
		var transactions = transactionsTask.Result;
		var accounts = accountsTask.Result;
		var counterparties = counterpartiesTask.Result;
		var counterparty = _gnomeshadeClient.GetMyCounterpartyAsync().ConfigureAwait(false).GetAwaiter().GetResult();

		var overviewTasks = transactions.Select(async transaction =>
		{
			var transfers = (await _gnomeshadeClient.GetTransfersAsync(transaction.Id))
				.Select(transfer => transfer.ToSummary(accounts, counterparties, counterparty))
				.ToList();

			return new TransactionOverview(
				transaction.Id,
				transaction.BookedAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset(),
				transaction.ValuedAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset(),
				transaction.Description,
				transaction.ImportedAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset(),
				transaction.ReconciledAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset(),
				transfers);
		});

		var overviews = await Task.WhenAll(overviewTasks).ConfigureAwait(false);

		var selected = Selected;
		var sort = DataGridView.SortDescriptions;
		Rows = new(overviews);
		DataGridView.SortDescriptions.AddRange(sort);
		Selected = selected;

		Filter.Accounts = accounts;
		Filter.Counterparties = counterparties;
		Summary.UpdateTotal(DataGridView.Cast<TransactionOverview>());
	}

	/// <inheritdoc />
	protected override async Task DeleteAsync(TransactionOverview row)
	{
		await _gnomeshadeClient.DeleteTransactionAsync(row.Id).ConfigureAwait(false);
		await RefreshAsync();
	}

	private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(Selected))
		{
			Details = TransactionUpsertionViewModel
				.CreateAsync(_gnomeshadeClient, _dateTimeZoneProvider, Selected?.Id)
				.ConfigureAwait(false)
				.GetAwaiter()
				.GetResult();
		}

		if (e.PropertyName is nameof(DataGridView))
		{
			DataGridView.Filter = Filter.Filter;
		}
	}

	private void FilterOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(TransactionFilter.IsValid))
		{
			OnPropertyChanged(nameof(CanRefresh));
		}

		if (e.PropertyName is nameof(TransactionFilter.SelectedAccount) or nameof(TransactionFilter.SelectedCounterparty))
		{
			DataGridView.Refresh();
			Summary.UpdateTotal(DataGridView.Cast<TransactionOverview>());
		}
	}

	private void DetailsOnUpserted(object? sender, UpsertedEventArgs e)
	{
		var updatedRow = Rows.SingleOrDefault(row => row.Id == e.Id);

		var transactionTask = _gnomeshadeClient.GetTransactionAsync(e.Id);
		var accountsTask = _gnomeshadeClient.GetAccountsAsync();
		var counterpartiesTask = _gnomeshadeClient.GetCounterpartiesAsync();

		Task.WhenAll(transactionTask, accountsTask, counterpartiesTask).ConfigureAwait(false).GetAwaiter().GetResult();
		var transaction = transactionTask.Result;
		var accounts = accountsTask.Result;
		var counterparties = counterpartiesTask.Result;
		var counterparty = _gnomeshadeClient.GetMyCounterpartyAsync().ConfigureAwait(false).GetAwaiter().GetResult();

		var transfers = _gnomeshadeClient
			.GetTransfersAsync(transaction.Id)
			.ConfigureAwait(false)
			.GetAwaiter()
			.GetResult()
			.Select(transfer => transfer.ToSummary(accounts, counterparties, counterparty))
			.ToList();

		var overview = new TransactionOverview(
			transaction.Id,
			transaction.BookedAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset(),
			transaction.ValuedAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset(),
			transaction.Description,
			transaction.ImportedAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset(),
			transaction.ReconciledAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset(),
			transfers);

		var sort = DataGridView.SortDescriptions;
		Rows = new(Rows.Where(row => row.Id != updatedRow?.Id).Append(overview).ToList());
		DataGridView.SortDescriptions.AddRange(sort);
		Selected = overview;
	}
}
