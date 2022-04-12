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

namespace Gnomeshade.Interfaces.Avalonia.Core.Transactions;

/// <summary>Overview of all <see cref="Transaction"/>s.</summary>
public sealed class TransactionViewModel : OverviewViewModel<TransactionOverview, TransactionUpsertionViewModel?>
{
	private readonly IGnomeshadeClient _gnomeshadeClient;

	private TransactionUpsertionViewModel? _details;

	private TransactionViewModel(IGnomeshadeClient gnomeshadeClient)
	{
		_gnomeshadeClient = gnomeshadeClient;

		PropertyChanged += OnPropertyChanged;

		Filter = new();
		Filter.PropertyChanged += FilterOnPropertyChanged;
	}

	/// <summary>Gets the transaction filter.</summary>
	public TransactionFilter Filter { get; }

	/// <summary>Gets a value indicating whether transactions can be refreshed.</summary>
	public bool CanRefresh => Filter.IsValid;

	/// <inheritdoc />
	public override TransactionUpsertionViewModel? Details
	{
		get => _details;
		set
		{
			if (_details is not null)
			{
				_details.Upserted -= DetailsOnUpserted;
			}

			SetAndNotify(ref _details, value);
			if (_details is not null)
			{
				_details.Upserted += DetailsOnUpserted;
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="TransactionViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">A strongly typed API client.</param>
	/// <returns>A new instance of the <see cref="TransactionViewModel"/> class.</returns>
	public static async Task<TransactionViewModel> CreateAsync(IGnomeshadeClient gnomeshadeClient)
	{
		var viewModel = new TransactionViewModel(gnomeshadeClient);
		await viewModel.RefreshAsync().ConfigureAwait(false);
		return viewModel;
	}

	/// <inheritdoc />
	public override async Task RefreshAsync()
	{
		var transactionsTask = _gnomeshadeClient.GetTransactionsAsync(Filter.FromDate, Filter.ToDate);
		var accountsTask = _gnomeshadeClient.GetAccountsAsync();
		var currenciesTask = _gnomeshadeClient.GetCurrenciesAsync();
		var productsTask = _gnomeshadeClient.GetProductsAsync();

		await Task.WhenAll(transactionsTask, accountsTask, currenciesTask, productsTask).ConfigureAwait(false);
		var transactions = transactionsTask.Result;
		var accounts = accountsTask.Result;
		var currencies = currenciesTask.Result;
		var products = productsTask.Result;

		var overviewTasks = transactions.Select(async transaction =>
		{
			var transfers = (await _gnomeshadeClient.GetTransfersAsync(transaction.Id))
				.Select(transfer => transfer.ToOverview(accounts))
				.ToList();

			var purchases = (await _gnomeshadeClient.GetPurchasesAsync(transaction.Id))
				.Select(purchase => purchase.ToOverview(currencies, products))
				.ToList();

			return new TransactionOverview(
				transaction.Id,
				transaction.BookedAt?.ToLocalTime(),
				transaction.ValuedAt?.ToLocalTime(),
				transaction.Description,
				transaction.ImportedAt?.ToLocalTime(),
				transaction.ReconciledAt?.ToLocalTime(),
				transfers,
				purchases);
		});

		var overviews = await Task.WhenAll(overviewTasks).ConfigureAwait(false);

		var sort = DataGridView.SortDescriptions;
		Rows = new(overviews);
		DataGridView.SortDescriptions.AddRange(sort);
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
			Details = Selected is null
				? null
				: TransactionUpsertionViewModel.CreateAsync(_gnomeshadeClient, Selected.Id).Result;
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
		var updatedRow = Rows.Single(row => row.Id == e.Id);

		var transactionTask = _gnomeshadeClient.GetTransactionAsync(updatedRow.Id);
		var accountsTask = _gnomeshadeClient.GetAccountsAsync();
		var currenciesTask = _gnomeshadeClient.GetCurrenciesAsync();
		var productsTask = _gnomeshadeClient.GetProductsAsync();

		Task.WhenAll(transactionTask, accountsTask, currenciesTask, productsTask).ConfigureAwait(false).GetAwaiter()
			.GetResult();
		var transaction = transactionTask.Result;
		var accounts = accountsTask.Result;
		var currencies = currenciesTask.Result;
		var products = productsTask.Result;

		var transfers = _gnomeshadeClient
			.GetTransfersAsync(transaction.Id)
			.ConfigureAwait(false)
			.GetAwaiter()
			.GetResult()
			.Select(transfer => transfer.ToOverview(accounts))
			.ToList();

		var purchases = _gnomeshadeClient.GetPurchasesAsync(transaction.Id)
			.ConfigureAwait(false)
			.GetAwaiter()
			.GetResult()
			.Select(purchase => purchase.ToOverview(currencies, products))
			.ToList();

		var overview = new TransactionOverview(
			transaction.Id,
			transaction.BookedAt?.ToLocalTime(),
			transaction.ValuedAt?.ToLocalTime(),
			transaction.Description,
			transaction.ImportedAt?.ToLocalTime(),
			transaction.ReconciledAt?.ToLocalTime(),
			transfers,
			purchases);

		var sort = DataGridView.SortDescriptions;
		Rows = new(Rows.Except(new[] { updatedRow }).Append(overview).ToList());
		DataGridView.SortDescriptions.AddRange(sort);
	}
}
