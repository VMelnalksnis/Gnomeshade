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
public sealed class TransactionViewModel : OverviewViewModel<TransactionOverview>
{
	private readonly IGnomeshadeClient _gnomeshadeClient;

	private TransactionViewModel(IGnomeshadeClient gnomeshadeClient)
	{
		_gnomeshadeClient = gnomeshadeClient;

		Filter = new();
		Filter.PropertyChanged += FilterOnPropertyChanged;
	}

	/// <summary>Gets the transaction filter.</summary>
	public TransactionFilter Filter { get; }

	/// <summary>Gets a value indicating whether transactions can be refreshed.</summary>
	public bool CanRefresh => Filter.IsValid;

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
		var transactions = await _gnomeshadeClient.GetTransactionsAsync(Filter.FromDate, Filter.ToDate).ConfigureAwait(false);
		var accountsTask = _gnomeshadeClient.GetAccountsAsync();
		var currenciesTask = _gnomeshadeClient.GetCurrenciesAsync();
		var productsTask = _gnomeshadeClient.GetProductsAsync();

		await Task.WhenAll(accountsTask, currenciesTask, productsTask).ConfigureAwait(false);
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
				transaction.BookedAt,
				transaction.ValuedAt,
				transaction.Description,
				transaction.ImportedAt,
				transaction.ReconciledAt,
				transfers,
				purchases);
		});

		var overviews = await Task.WhenAll(overviewTasks).ConfigureAwait(false);

		Rows = new(overviews); // todo sorting
	}

	/// <summary>Handles the <see cref="InputElement.DoubleTapped"/> event for <see cref="OverviewViewModel{TRow}.DataGridView"/>.</summary>
	public void OnDataGridDoubleTapped()
	{
	}

	private void FilterOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(TransactionFilter.IsValid))
		{
			OnPropertyChanged(nameof(CanRefresh));
		}
	}
}
