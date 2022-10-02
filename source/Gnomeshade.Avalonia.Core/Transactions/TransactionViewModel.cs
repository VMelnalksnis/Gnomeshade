// Copyright 2021 Valters Melnalksnis
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
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly IDialogService _dialogService;
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider;

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
		_dateTimeZoneProvider = dateTimeZoneProvider;
		_details = new(activityService, _gnomeshadeClient, _dialogService, _dateTimeZoneProvider, null);

		_details.Upserted += DetailsOnUpserted;
		PropertyChanged += OnPropertyChanged;

		Filter = new(ActivityService, clock, dateTimeZoneProvider);
		Filter.PropertyChanged += FilterOnPropertyChanged;
		DataGridView.Filter = Filter.Filter;
		DataGridView.SortDescriptions.Add(
			new DataGridComparerSortDesctiption(
				new TransactionOverviewComparer(overview => overview?.Date),
				ListSortDirection.Descending));
		DataGridView.SortDescriptions.Add(
			new DataGridComparerSortDesctiption(
				new TransactionOverviewComparer(overview => overview?.ReconciledAt),
				ListSortDirection.Descending));

		Summary = new(ActivityService);
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

	/// <inheritdoc />
	public override async Task UpdateSelection()
	{
		Details = new(ActivityService, _gnomeshadeClient, _dialogService, _dateTimeZoneProvider, Selected?.Id);
		await Details.RefreshAsync();
	}

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		var transactionsTask = _gnomeshadeClient.GetDetailedTransactionsAsync(Filter.Interval);
		var accountsTask = _gnomeshadeClient.GetAccountsAsync();
		var counterpartiesTask = _gnomeshadeClient.GetCounterpartiesAsync();
		var productsTask = _gnomeshadeClient.GetProductsAsync();

		await Task.WhenAll(transactionsTask, accountsTask, counterpartiesTask, productsTask);
		var transactions = transactionsTask.Result;
		var accounts = accountsTask.Result;
		var counterparties = counterpartiesTask.Result;
		var counterparty = await _gnomeshadeClient.GetMyCounterpartyAsync();

		var overviews = transactions.Select(transaction =>
		{
			var transfers = transaction.Transfers
				.Select(transfer => transfer.ToSummary(accounts, counterparties, counterparty))
				.ToList();

			return new TransactionOverview(
				transaction.Id,
				transaction.BookedAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset(),
				transaction.ValuedAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset(),
				transaction.ReconciledAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset(),
				transfers,
				transaction.Purchases);
		}).ToList();

		var selected = Selected;
		var sort = DataGridView.SortDescriptions;
		Rows = new(overviews);
		DataGridView.SortDescriptions.AddRange(sort);
		Selected = selected;

		Filter.Accounts = accounts;
		Filter.Counterparties = counterparties;
		Filter.Products = productsTask.Result;
		Summary.UpdateTotal(DataGridView.Cast<TransactionOverview>());
	}

	/// <inheritdoc />
	protected override async Task DeleteAsync(TransactionOverview row)
	{
		await _gnomeshadeClient.DeleteTransactionAsync(row.Id);
		await RefreshAsync();
	}

	private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
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

		if (e.PropertyName is
			nameof(TransactionFilter.SelectedAccount) or
			nameof(TransactionFilter.InvertAccount) or
			nameof(TransactionFilter.SelectedCounterparty) or
			nameof(TransactionFilter.InvertCounterparty) or
			nameof(TransactionFilter.SelectedProduct) or
			nameof(TransactionFilter.InvertProduct))
		{
			DataGridView.Refresh();
			Summary.UpdateTotal(DataGridView.Cast<TransactionOverview>());
		}
	}

	private async void DetailsOnUpserted(object? sender, UpsertedEventArgs e)
	{
		var updatedOverview = Rows.SingleOrDefault(overview => overview.Id == e.Id);

		var transactionTask = _gnomeshadeClient.GetDetailedTransactionAsync(e.Id);
		var accountsTask = _gnomeshadeClient.GetAccountsAsync();
		var counterpartiesTask = _gnomeshadeClient.GetCounterpartiesAsync();

		await Task.WhenAll(transactionTask, accountsTask, counterpartiesTask);
		var transaction = transactionTask.Result;
		var accounts = accountsTask.Result;
		var counterparties = counterpartiesTask.Result;
		var counterparty = await _gnomeshadeClient.GetMyCounterpartyAsync();

		var transfers = transaction
			.Transfers
			.Select(transfer => transfer.ToSummary(accounts, counterparties, counterparty))
			.ToList();

		var overview = new TransactionOverview(
			transaction.Id,
			transaction.BookedAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset(),
			transaction.ValuedAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset(),
			transaction.ReconciledAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset(),
			transfers,
			transaction.Purchases);

		var sort = DataGridView.SortDescriptions;
		Rows = new(Rows.Where(row => row.Id != updatedOverview?.Id).Append(overview).ToList());
		DataGridView.SortDescriptions.AddRange(sort);
		Selected = overview;
	}
}
