// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;

namespace Gnomeshade.Interfaces.Avalonia.Core.Transactions.Transfers;

/// <summary>Overview of all <see cref="Transfer"/>s of a single <see cref="Transaction"/>.</summary>
public sealed class TransferViewModel : OverviewViewModel<TransferOverview, TransferUpsertionViewModel>
{
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly Guid _transactionId;

	private TransferUpsertionViewModel _details;

	private TransferViewModel(
		IGnomeshadeClient gnomeshadeClient,
		Guid transactionId,
		TransferUpsertionViewModel details)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_transactionId = transactionId;
		_details = details;

		PropertyChanged += OnPropertyChanged;
		_details.Upserted += DetailsOnUpserted;
	}

	/// <summary>Gets the total transferred amount.</summary>
	public decimal Total => Rows.Select(overview => overview.TargetAmount).Sum();

	/// <inheritdoc />
	public override TransferUpsertionViewModel Details
	{
		get => _details;
		set
		{
			_details.Upserted -= DetailsOnUpserted;
			SetAndNotify(ref _details, value);
			_details.Upserted += DetailsOnUpserted;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="TransferViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">A strongly typed API client.</param>
	/// <param name="transactionId">The transaction for which to create a transfer overview.</param>
	/// <returns>A new instance of the <see cref="TransferViewModel"/> class.</returns>
	public static async Task<TransferViewModel> CreateAsync(IGnomeshadeClient gnomeshadeClient, Guid transactionId)
	{
		var upsertionViewModel = await TransferUpsertionViewModel.CreateAsync(gnomeshadeClient, transactionId).ConfigureAwait(false);
		var viewModel = new TransferViewModel(gnomeshadeClient, transactionId, upsertionViewModel);
		await viewModel.RefreshAsync().ConfigureAwait(false);
		SetDefaultCurrency(viewModel);
		return viewModel;
	}

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		var transactionsTask = _gnomeshadeClient.GetDetailedTransactionAsync(_transactionId);
		var accountsTask = _gnomeshadeClient.GetAccountsAsync();
		await Task.WhenAll(transactionsTask, accountsTask).ConfigureAwait(false);

		var accounts = accountsTask.Result;
		var transaction = transactionsTask.Result;
		IsReadOnly = transaction.Reconciled;

		var overviews = transaction.Transfers
			.OrderBy(transfer => transfer.CreatedAt)
			.Select(transfer => transfer.ToOverview(accounts));

		var selected = Selected;
		var sort = DataGridView.SortDescriptions;
		Rows.CollectionChanged -= RowsOnCollectionChanged;
		Rows = new(overviews); // todo sorting
		Rows.CollectionChanged += RowsOnCollectionChanged;
		DataGridView.SortDescriptions.AddRange(sort);
		Selected = Rows.SingleOrDefault(overview => overview.Id == selected?.Id);
	}

	/// <inheritdoc />
	protected override async Task DeleteAsync(TransferOverview row)
	{
		await _gnomeshadeClient.DeleteTransferAsync(_transactionId, row.Id).ConfigureAwait(false);
		await RefreshAsync();
	}

	private static void SetDefaultCurrency(TransferViewModel viewModel)
	{
		if (viewModel.Rows.Select(overview => overview.SourceCurrency).Distinct().Count() == 1)
		{
			var currencyName = viewModel.Rows.First().SourceCurrency;
			viewModel.Details.SourceCurrency = viewModel.Details.Currencies.Single(currency => currency.AlphabeticCode == currencyName);
		}

		if (viewModel.Rows.Select(overview => overview.TargetCurrency).Distinct().Count() == 1)
		{
			var currencyName = viewModel.Rows.First().TargetCurrency;
			viewModel.Details.TargetCurrency = viewModel.Details.Currencies.Single(currency => currency.AlphabeticCode == currencyName);
		}
	}

	private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(Selected))
		{
			Details = TransferUpsertionViewModel
				.CreateAsync(_gnomeshadeClient, _transactionId, Selected?.Id)
				.ConfigureAwait(false)
				.GetAwaiter()
				.GetResult();

			SetDefaultCurrency(this);
		}

		if (e.PropertyName is nameof(Rows))
		{
			OnPropertyChanged(nameof(Total));
		}
	}

	private void RowsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		OnPropertyChanged(nameof(Total));
	}

	private void DetailsOnUpserted(object? sender, UpsertedEventArgs e)
	{
		RefreshAsync().ConfigureAwait(false).GetAwaiter().GetResult();
	}
}
