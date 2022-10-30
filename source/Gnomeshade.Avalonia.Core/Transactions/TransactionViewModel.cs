﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Collections;

using Gnomeshade.Avalonia.Core.Transactions.Controls;
using Gnomeshade.Avalonia.Core.Transactions.Transfers;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Transactions;

using NodaTime;

namespace Gnomeshade.Avalonia.Core.Transactions;

/// <summary>Overview of all <see cref="Transaction"/>s.</summary>
public sealed class TransactionViewModel : OverviewViewModel<TransactionOverview, TransactionUpsertionViewModel>
{
	private static readonly DataGridSortDescription[] _sortDescriptions =
	{
		DataGridSortDescription.FromComparer(
			new TransactionOverviewComparer(overview => overview?.Date),
			ListSortDirection.Descending),

		DataGridSortDescription.FromComparer(
			new TransactionOverviewComparer(overview => overview?.ReconciledAt),
			ListSortDirection.Descending),
	};

	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly IDialogService _dialogService;
	private readonly IClock _clock;
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider;

	private bool _disposed;
	private TransactionUpsertionViewModel _details;

	/// <summary>Initializes a new instance of the <see cref="TransactionViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">A strongly typed API client.</param>
	/// <param name="dialogService">Service for creating dialog windows.</param>
	/// <param name="clock">Clock which can provide the current instant.</param>
	/// <param name="dateTimeZoneProvider">Time zone provider for localizing instants to local time.</param>
	public TransactionViewModel(
		IActivityService activityService,
		IGnomeshadeClient gnomeshadeClient,
		IDialogService dialogService,
		IClock clock,
		IDateTimeZoneProvider dateTimeZoneProvider)
		: base(activityService)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_dialogService = dialogService;
		_clock = clock;
		_dateTimeZoneProvider = dateTimeZoneProvider;
		_details = new(activityService, _gnomeshadeClient, _dialogService, _clock, _dateTimeZoneProvider, null);

		_details.Upserted += DetailsOnUpserted;
		PropertyChanged += OnPropertyChanged;

		Filter = new(ActivityService, clock, dateTimeZoneProvider);
		Filter.PropertyChanged += FilterOnPropertyChanged;
		DataGridView.Filter = Filter.Filter;
		DataGridView.SortDescriptions.AddRange(_sortDescriptions);

		Summary = new(ActivityService);
		Merge = new(ActivityService, _gnomeshadeClient);
		Refund = new(ActivityService, _gnomeshadeClient);
	}

	/// <summary>Gets the transaction filter.</summary>
	public TransactionFilter Filter { get; }

	/// <summary>Gets the summary of displayed transactions.</summary>
	public TransactionSummary Summary { get; }

	/// <summary>Gets the model for merging transactions.</summary>
	public TransactionMerge Merge { get; }

	/// <summary>Gets the model for refunding transactions.</summary>
	public TransactionRefund Refund { get; }

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

	/// <summary>Gets a value indicating whether the source transaction for merging can be selected.</summary>
	public bool CanSelectSource => Selected is not null;

	/// <summary>Gets a value indicating whether the target transaction for merging can be selected.</summary>
	public bool CanSelectTarget => Selected is not null;

	/// <inheritdoc />
	public override async Task UpdateSelection()
	{
		Details = new(ActivityService, _gnomeshadeClient, _dialogService, _clock, _dateTimeZoneProvider, Selected?.Id);
		await Details.RefreshAsync();
	}

	/// <summary>Sets the currently <see cref="OverviewViewModel{TRow,TUpsertion}.Selected"/> transaction as the source for merging.</summary>
	public void SelectSource()
	{
		Merge.Source = Selected;
		Refund.Source = Selected;
	}

	/// <summary>Sets the currently <see cref="OverviewViewModel{TRow,TUpsertion}.Selected"/> transaction as the target for merging.</summary>
	public void SelectTarget()
	{
		Merge.Target = Selected;
		Refund.Target = Selected;
	}

	/// <summary>Clears mergeable transactions.</summary>
	public void ClearMerge()
	{
		Merge.Source = null;
		Merge.Target = null;

		Refund.Source = null;
		Refund.Target = null;
	}

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		var transactionsTask = _gnomeshadeClient.GetDetailedTransactionsAsync(Filter.Interval);
		var accountsTask = _gnomeshadeClient.GetAccountsAsync();
		var counterpartiesTask = _gnomeshadeClient.GetCounterpartiesAsync();
		var productsTask = _gnomeshadeClient.GetProductsAsync();
		var categoriesTask = _gnomeshadeClient.GetCategoriesAsync();
		var currenciesTask = _gnomeshadeClient.GetCurrenciesAsync();
		var counterpartyTask = _gnomeshadeClient.GetMyCounterpartyAsync();

		await Task.WhenAll(transactionsTask, accountsTask, counterpartiesTask, productsTask, categoriesTask, currenciesTask, counterpartyTask);
		var transactions = transactionsTask.Result;
		var accounts = accountsTask.Result;
		var accountsInCurrency = accounts.SelectMany(account => account.Currencies.Select(currency => (AccountInCurrency: currency, Account: account))).ToArray();
		var counterparties = counterpartiesTask.Result;

		var overviews = transactions.Select(transaction =>
		{
			var transfers = transaction.Transfers
				.OrderBy(transfer => transfer.Order)
				.Select(transfer => transfer.ToSummary(counterparties, counterpartyTask.Result, accountsInCurrency))
				.ToList();

			return new TransactionOverview(
				transaction.Id,
				transaction.BookedAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset(),
				transaction.ValuedAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset(),
				transaction.ReconciledAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset(),
				transfers,
				transaction.Purchases,
				transaction.Refunded);
		}).ToList();

		var selected = Selected;
		var sort = DataGridView.SortDescriptions;
		Rows = new(overviews);
		DataGridView.SortDescriptions.AddRange(sort);
		Selected = selected;

		Filter.Accounts = accounts;
		Filter.Counterparties = counterparties;
		Filter.Products = productsTask.Result;
		Filter.Categories = categoriesTask.Result;
		Summary.UpdateTotal(Rows);
	}

	/// <inheritdoc />
	protected override async Task DeleteAsync(TransactionOverview row)
	{
		await _gnomeshadeClient.DeleteTransactionAsync(row.Id);
		await RefreshAsync();
	}

	/// <inheritdoc />
	protected override void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing)
			{
				_details.Upserted -= DetailsOnUpserted;
				PropertyChanged -= OnPropertyChanged;
				Filter.PropertyChanged -= FilterOnPropertyChanged;
			}

			_disposed = true;
		}

		base.Dispose(disposing);
	}

	private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(DataGridView))
		{
			DataGridView.Filter = Filter.Filter;
		}

		if (e.PropertyName is nameof(Selected))
		{
			OnPropertyChanged(nameof(CanSelectSource));
			OnPropertyChanged(nameof(CanSelectTarget));
		}
	}

	private void FilterOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(TransactionFilter.IsValid))
		{
			OnPropertyChanged(nameof(CanRefresh));
		}

		if (e.PropertyName is
			nameof(TransactionFilter.SelectedAccount) or
			nameof(TransactionFilter.InvertAccount) or
			nameof(TransactionFilter.SelectedCounterparty) or
			nameof(TransactionFilter.InvertCounterparty) or
			nameof(TransactionFilter.SelectedProduct) or
			nameof(TransactionFilter.InvertProduct) or
			nameof(TransactionFilter.SelectedCategory) or
			nameof(TransactionFilter.InvertCategory) or
			nameof(TransactionFilter.Reconciled) or
			nameof(TransactionFilter.Uncategorized))
		{
			DataGridView.Refresh();
			Summary.UpdateTotal(Rows);
		}
	}

	private async void DetailsOnUpserted(object? sender, UpsertedEventArgs e)
	{
		var updatedOverview = Rows.SingleOrDefault(overview => overview.Id == e.Id);

		var transactionTask = _gnomeshadeClient.GetDetailedTransactionAsync(e.Id);
		var accountsTask = _gnomeshadeClient.GetAccountsAsync();
		var counterpartiesTask = _gnomeshadeClient.GetCounterpartiesAsync();
		var currenciesTask = _gnomeshadeClient.GetCurrenciesAsync();
		var counterpartyTask = _gnomeshadeClient.GetMyCounterpartyAsync();

		await Task.WhenAll(transactionTask, accountsTask, counterpartiesTask, currenciesTask, counterpartyTask);
		var transaction = transactionTask.Result;
		var accounts = accountsTask.Result;
		var accountsInCurrency = accounts.SelectMany(account => account.Currencies.Select(currency => (AccountInCurrency: currency, Account: account))).ToArray();
		var counterparties = counterpartiesTask.Result;

		var transfers = transaction.Transfers
			.OrderBy(transfer => transfer.Order)
			.Select(transfer => transfer.ToSummary(counterparties, counterpartyTask.Result, accountsInCurrency))
			.ToList();

		var overview = new TransactionOverview(
			transaction.Id,
			transaction.BookedAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset(),
			transaction.ValuedAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset(),
			transaction.ReconciledAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset(),
			transfers,
			transaction.Purchases,
			transaction.Refunded);

		var sort = DataGridView.SortDescriptions;
		Rows = new(Rows.Where(row => row.Id != updatedOverview?.Id).Append(overview).ToList());
		DataGridView.SortDescriptions.AddRange(sort);
		Selected = overview;
	}
}
