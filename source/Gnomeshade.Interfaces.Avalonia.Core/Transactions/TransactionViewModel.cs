// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Input;

using Gnomeshade.Interfaces.Avalonia.Core.Transactions.Controls;
using Gnomeshade.Interfaces.Avalonia.Core.Transactions.Purchases;
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
		IDateTimeZoneProvider dateTimeZoneProvider,
		TransactionUpsertionViewModel details)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_dateTimeZoneProvider = dateTimeZoneProvider;
		_details = details;

		_details.Upserted += DetailsOnUpserted;
		PropertyChanged += OnPropertyChanged;

		Filter = new();
		Filter.PropertyChanged += FilterOnPropertyChanged;
	}

	/// <summary>Gets the transaction filter.</summary>
	public TransactionFilter Filter { get; }

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
	/// <param name="dateTimeZoneProvider">Time zone provider for localizing instants to local time.</param>
	/// <returns>A new instance of the <see cref="TransactionViewModel"/> class.</returns>
	public static async Task<TransactionViewModel> CreateAsync(
		IGnomeshadeClient gnomeshadeClient,
		IDateTimeZoneProvider dateTimeZoneProvider)
	{
		var upsertionViewModel = await TransactionUpsertionViewModel.CreateAsync(gnomeshadeClient, dateTimeZoneProvider).ConfigureAwait(false);
		var viewModel = new TransactionViewModel(gnomeshadeClient, dateTimeZoneProvider, upsertionViewModel);
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
		var currenciesTask = _gnomeshadeClient.GetCurrenciesAsync();
		var productsTask = _gnomeshadeClient.GetProductsAsync();

		await Task.WhenAll(transactionsTask, accountsTask, currenciesTask, productsTask).ConfigureAwait(false);
		var transactions = transactionsTask.Result;
		var accounts = accountsTask.Result;
		var currencies = currenciesTask.Result;
		var products = productsTask.Result;
		var counterparty = _gnomeshadeClient.GetMyCounterpartyAsync().ConfigureAwait(false).GetAwaiter().GetResult();

		var overviewTasks = transactions.Select(async transaction =>
		{
			var transfers = (await _gnomeshadeClient.GetTransfersAsync(transaction.Id))
				.Select(transfer => transfer.ToSummary(accounts, counterparty))
				.ToList();

			var purchases = (await _gnomeshadeClient.GetPurchasesAsync(transaction.Id))
				.Select(purchase => purchase.ToOverview(currencies, products, _dateTimeZoneProvider))
				.ToList();

			return new TransactionOverview(
				transaction.Id,
				transaction.BookedAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset(),
				transaction.ValuedAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset(),
				transaction.Description,
				transaction.ImportedAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset(),
				transaction.ReconciledAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset(),
				transfers,
				purchases);
		});

		var overviews = await Task.WhenAll(overviewTasks).ConfigureAwait(false);

		var selected = Selected;
		var sort = DataGridView.SortDescriptions;
		Rows = new(overviews);
		DataGridView.SortDescriptions.AddRange(sort);
		Selected = selected;
	}

	/// <summary>Handles the <see cref="InputElement.DoubleTapped"/> event for <see cref="OverviewViewModel{TRow,TUpsertion}.DataGridView"/>.</summary>
	public void OnDataGridDoubleTapped()
	{
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
	}

	private void FilterOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(TransactionFilter.IsValid))
		{
			OnPropertyChanged(nameof(CanRefresh));
		}
	}

	private void DetailsOnUpserted(object? sender, UpsertedEventArgs e)
	{
		var updatedRow = Rows.SingleOrDefault(row => row.Id == e.Id);

		var transactionTask = _gnomeshadeClient.GetTransactionAsync(e.Id);
		var accountsTask = _gnomeshadeClient.GetAccountsAsync();
		var currenciesTask = _gnomeshadeClient.GetCurrenciesAsync();
		var productsTask = _gnomeshadeClient.GetProductsAsync();

		Task.WhenAll(transactionTask, accountsTask, currenciesTask, productsTask).ConfigureAwait(false).GetAwaiter().GetResult();
		var transaction = transactionTask.Result;
		var accounts = accountsTask.Result;
		var currencies = currenciesTask.Result;
		var products = productsTask.Result;
		var counterparty = _gnomeshadeClient.GetMyCounterpartyAsync().ConfigureAwait(false).GetAwaiter().GetResult();

		var transfers = _gnomeshadeClient
			.GetTransfersAsync(transaction.Id)
			.ConfigureAwait(false)
			.GetAwaiter()
			.GetResult()
			.Select(transfer => transfer.ToSummary(accounts, counterparty))
			.ToList();

		var purchases = _gnomeshadeClient.GetPurchasesAsync(transaction.Id)
			.ConfigureAwait(false)
			.GetAwaiter()
			.GetResult()
			.Select(purchase => purchase.ToOverview(currencies, products, _dateTimeZoneProvider))
			.ToList();

		var overview = new TransactionOverview(
			transaction.Id,
			transaction.BookedAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset(),
			transaction.ValuedAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset(),
			transaction.Description,
			transaction.ImportedAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset(),
			transaction.ReconciledAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset(),
			transfers,
			purchases);

		var sort = DataGridView.SortDescriptions;
		Rows = new(Rows.Where(row => row.Id != updatedRow?.Id).Append(overview).ToList());
		DataGridView.SortDescriptions.AddRange(sort);
		Selected = overview;
	}
}
