// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Collections;

using Gnomeshade.Avalonia.Core.Commands;
using Gnomeshade.Avalonia.Core.Transactions.Controls;
using Gnomeshade.Avalonia.Core.Transactions.Transfers;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Transactions;

using NodaTime;

using PropertyChanged.SourceGenerator;

using static System.ComponentModel.ListSortDirection;

namespace Gnomeshade.Avalonia.Core.Transactions;

/// <summary>Overview of all <see cref="Transaction"/>s.</summary>
public sealed partial class TransactionViewModel : OverviewViewModel<TransactionOverview, TransactionUpsertionBase>
{
	private static readonly DataGridSortDescription[] _sortDescriptions =
	[
		DataGridSortDescription.FromComparer(new TransactionOverviewComparer(overview => overview?.Date), Descending),
		DataGridSortDescription.FromComparer(new TransactionOverviewComparer(overview => overview?.ReconciledAt), Descending)
	];

	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly IDialogService _dialogService;
	private readonly IClock _clock;
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider;

	private readonly TransactionMerge _merge;

	private bool _disposed;
	private TransactionUpsertionBase _details;

	/// <summary>Gets all selected transactions.</summary>
	[Notify(Setter.Private)]
	private List<TransactionOverview> _selectedItems = [];

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
		_merge = new(ActivityService, _gnomeshadeClient);
		_details = new TransactionUpsertionViewModel(activityService, _gnomeshadeClient, _dialogService, _clock, _dateTimeZoneProvider, null);

		_details.Upserted += DetailsOnUpserted;
		PropertyChanging += OnPropertyChanging;
		PropertyChanged += OnPropertyChanged;

		Filter = new(ActivityService, clock, dateTimeZoneProvider);
		Filter.PropertyChanged += FilterOnPropertyChanged;
		DataGridView.Filter = Filter.Filter;
		DataGridView.SortDescriptions.AddRange(_sortDescriptions);

		Summary = new(ActivityService);
		Refund = new(ActivityService, _gnomeshadeClient);

		Merge = ActivityService.Create(MergeAsync, () => _merge.CanMerge, "Merging transactions");
		UpdateSelectedItems = ActivityService.Create<IList>(UpdateSelectedItemsAsync, "Updating selected items");
	}

	/// <summary>Gets a command for merging selected transactions.</summary>
	public CommandBase Merge { get; }

	/// <summary>Gets a command that updates view model state based on selected rows.</summary>
	public CommandBase UpdateSelectedItems { get; }

	/// <summary>Gets the transaction filter.</summary>
	public TransactionFilter Filter { get; }

	/// <summary>Gets the summary of displayed transactions.</summary>
	public TransactionSummary Summary { get; }

	/// <summary>Gets the model for refunding transactions.</summary>
	public TransactionRefund Refund { get; }

	/// <summary>Gets a value indicating whether transactions can be refreshed.</summary>
	public bool CanRefresh => Filter.IsValid;

	/// <summary>Gets a value indicating whether the <see cref="TransferSummary.UserCurrency"/> column needs to be shown.</summary>
	public bool ShowUserCurrency => Rows
		.SelectMany(transaction => transaction.Transfers)
		.Any(transfer => !string.IsNullOrWhiteSpace(transfer.UserCurrency));

	/// <summary>
	/// Gets a value indicating whether the <see cref="TransferSummary.OtherCurrency"/> and
	/// <see cref="TransferSummary.OtherAmount"/> columns needs to be shown.
	/// </summary>
	public bool ShowOtherAmount => Rows
		.SelectMany(transaction => transaction.Transfers)
		.Any(transfer => transfer.DisplayTarget);

	/// <inheritdoc />
	public override TransactionUpsertionBase Details
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
	public bool CanSelectSource => SelectedItems is not [];

	/// <summary>Gets a value indicating whether the target transaction for merging can be selected.</summary>
	public bool CanSelectTarget => SelectedItem is not null;

	private TransactionOverview? SelectedItem => SelectedItems is [var overview] ? overview : null;

	/// <inheritdoc />
	public override async Task UpdateSelection()
	{
		if (Selected is { Projection: true })
		{
			Details = new PlannedTransactionUpsertionViewModel(ActivityService, _gnomeshadeClient, _dialogService, _dateTimeZoneProvider, Selected?.Id);
		}
		else
		{
			Details = new TransactionUpsertionViewModel(ActivityService, _gnomeshadeClient, _dialogService, _clock, _dateTimeZoneProvider, SelectedItem?.Id);
		}

		await Details.RefreshAsync();
	}

	/// <summary>Sets the currently <see cref="OverviewViewModel{TRow,TUpsertion}.Selected"/> transaction as the source for merging.</summary>
	public void SelectSource()
	{
		_merge.Sources = SelectedItems;
		Merge.InvokeExecuteChanged();
		Refund.Source = SelectedItem;
	}

	/// <summary>Sets the currently <see cref="OverviewViewModel{TRow,TUpsertion}.Selected"/> transaction as the target for merging.</summary>
	public void SelectTarget()
	{
		_merge.Target = SelectedItem;
		Merge.InvokeExecuteChanged();
		Refund.Target = SelectedItem;
	}

	/// <summary>Clears mergeable transactions.</summary>
	public void ClearMerge()
	{
		_merge.Sources = [];
		_merge.Target = null;
		Merge.InvokeExecuteChanged();

		Refund.Source = null;
		Refund.Target = null;
	}

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		var (transactions, plannedTransactions, accounts, counterparties, products, categories, counterparty, loans) = await (
			_gnomeshadeClient.GetDetailedTransactionsAsync(Filter.Interval),
			_gnomeshadeClient.GetPlannedTransactions(Filter.Interval),
			_gnomeshadeClient.GetAccountsAsync(),
			_gnomeshadeClient.GetCounterpartiesAsync(),
			_gnomeshadeClient.GetProductsAsync(),
			_gnomeshadeClient.GetCategoriesAsync(),
			_gnomeshadeClient.GetMyCounterpartyAsync(),
			_gnomeshadeClient.GetLoansAsync())
			.WhenAll();

		var accountsInCurrency = accounts
			.SelectMany(account => account.Currencies.Select(currency => (AccountInCurrency: currency, Account: account)))
			.ToArray();

		var overviews = transactions.Select(transaction =>
		{
			var transfers = transaction.Transfers
				.OrderBy(transfer => transfer.Order)
				.Select(transfer => transfer.ToSummary(counterparties, counterparty, accountsInCurrency))
				.ToList();

			return new TransactionOverview(
				transaction.Id,
				transaction.BookedAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset(),
				transaction.ValuedAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset(),
				transaction.ReconciledAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset(),
				transfers,
				transaction.Purchases,
				transaction.LoanPayments);
		}).ToList();

		if (Filter.IncludeProjections)
		{
			var currentTimeZone = _dateTimeZoneProvider.GetSystemDefault();
			var plannedOverviews = await Task.WhenAll(plannedTransactions
				.Select(async transaction =>
				{
					var plannedTransfers = await _gnomeshadeClient.GetPlannedTransfers(transaction.Id);
					var transfers = plannedTransfers
						.Select(plannedTransfer => plannedTransfer.ToSummary(plannedTransfer.BookedAt!.Value, counterparties, counterparty, accountsInCurrency))
						.ToList();

					var date = plannedTransfers
						.Select(transfer => transfer.BookedAt)
						.Max();

					var purchases = await _gnomeshadeClient.GetPlannedPurchases(transaction.Id);
					var loanPayments = await _gnomeshadeClient.GetPlannedLoanPayments(transaction.Id);

					return new TransactionOverview(
						transaction.Id,
						date!.Value.InZone(currentTimeZone).ToDateTimeOffset(),
						null,
						null,
						transfers,
						purchases,
						loanPayments,
						true);
				}));
			overviews = overviews.Concat(plannedOverviews).ToList();
		}

		var selected = SelectedItem;
		var sort = DataGridView.SortDescriptions;
		Rows = new(overviews);
		DataGridView.SortDescriptions.AddRange(sort);
		Selected = Rows.SingleOrDefault(overview => overview.Id == selected?.Id);

		Filter.Accounts = accounts;
		Filter.Counterparties = counterparties;
		Filter.Products = products;
		Filter.Categories = categories;
		Filter.Loans = loans;
		Summary.UpdateTotal(Rows);
	}

	/// <inheritdoc />
	protected override async Task DeleteAsync(TransactionOverview row)
	{
		await _gnomeshadeClient.DeleteTransactionAsync(row.Id);
		Details = new TransactionUpsertionViewModel(ActivityService, _gnomeshadeClient, _dialogService, _clock, _dateTimeZoneProvider, null);
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

	private async Task UpdateSelectedItemsAsync(IList list)
	{
		SelectedItems = list.Cast<TransactionOverview>().ToList();

		await UpdateSelection();
	}

	private async Task MergeAsync()
	{
		await _merge.MergeAsync();
		await RefreshAsync();
	}

	private void OnPropertyChanging(object? sender, PropertyChangingEventArgs e)
	{
		if (e.PropertyName is nameof(Rows))
		{
			OnPropertyChanging(nameof(ShowUserCurrency));
			OnPropertyChanging(nameof(ShowOtherAmount));
		}
	}

	private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		switch (e.PropertyName)
		{
			case nameof(DataGridView):
				DataGridView.Filter = Filter.Filter;
				break;

			case nameof(Selected):
			case nameof(SelectedItems):
				OnPropertyChanged(nameof(CanSelectSource));
				OnPropertyChanged(nameof(CanSelectTarget));
				break;

			case nameof(Rows):
				OnPropertyChanged(nameof(ShowUserCurrency));
				OnPropertyChanged(nameof(ShowOtherAmount));
				break;
		}
	}

	private async void FilterOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(TransactionFilter.IsValid))
		{
			OnPropertyChanged(nameof(CanRefresh));
		}

		if (e.PropertyName is nameof(TransactionFilter.IncludeProjections))
		{
			await RefreshAsync();
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
			nameof(TransactionFilter.SelectedLoan) or
			nameof(TransactionFilter.InvertLoan) or
			nameof(TransactionFilter.TransferReferenceFilter) or
			nameof(TransactionFilter.Reconciled) or
			nameof(TransactionFilter.Uncategorized))
		{
			DataGridView.Refresh();
			Summary.UpdateTotal(Rows);
		}
	}

	private async void DetailsOnUpserted(object? sender, UpsertedEventArgs e)
	{
		var (transaction, accounts, counterparties, counterparty) = await
			(_gnomeshadeClient.GetDetailedTransactionAsync(e.Id),
			_gnomeshadeClient.GetAccountsAsync(),
			_gnomeshadeClient.GetCounterpartiesAsync(),
			_gnomeshadeClient.GetMyCounterpartyAsync())
			.WhenAll();

		var accountsInCurrency = accounts
			.SelectMany(account => account.Currencies.Select(currency => (AccountInCurrency: currency, Account: account)))
			.ToArray();

		var transfers = transaction.Transfers
			.OrderBy(transfer => transfer.Order)
			.Select(transfer => transfer.ToSummary(counterparties, counterparty, accountsInCurrency))
			.ToList();

		var oldOverview = Rows.SingleOrDefault(overview => overview.Id == e.Id);
		var newOverview = new TransactionOverview(
			transaction.Id,
			transaction.BookedAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset(),
			transaction.ValuedAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset(),
			transaction.ReconciledAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset(),
			transfers,
			transaction.Purchases,
			transaction.LoanPayments);

		var sort = DataGridView.SortDescriptions;
		Rows = new(Rows.Where(overview => overview.Id != oldOverview?.Id).Append(newOverview).ToList());
		DataGridView.SortDescriptions.AddRange(sort);
		Selected = newOverview;
	}
}
