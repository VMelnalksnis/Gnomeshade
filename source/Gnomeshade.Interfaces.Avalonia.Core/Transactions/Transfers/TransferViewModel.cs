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
	}

	/// <summary>Gets the total transferred amount.</summary>
	public decimal Total => Rows.Select(overview => overview.TargetAmount).Sum();

	/// <inheritdoc />
	public override TransferUpsertionViewModel Details
	{
		get => _details;
		set => SetAndNotify(ref _details, value);
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
		return viewModel;
	}

	/// <inheritdoc />
	public override async Task RefreshAsync()
	{
		var transfersTask = _gnomeshadeClient.GetTransfersAsync(_transactionId);
		var accountsTask = _gnomeshadeClient.GetAccountsAsync();
		await Task.WhenAll(transfersTask, accountsTask).ConfigureAwait(false);

		var accounts = accountsTask.Result;
		var overviews = transfersTask.Result.Select(transfer => transfer.ToOverview(accounts));

		Rows.CollectionChanged -= RowsOnCollectionChanged;
		Rows = new(overviews); // todo sorting
		Rows.CollectionChanged += RowsOnCollectionChanged;
	}

	/// <inheritdoc />
	public override async Task DeleteSelectedAsync()
	{
		if (Selected is null)
		{
			throw new InvalidOperationException();
		}

		await _gnomeshadeClient.DeleteTransferAsync(_transactionId, Selected.Id).ConfigureAwait(false);
		await RefreshAsync();
	}

	private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(Selected))
		{
			Details = TransferUpsertionViewModel.CreateAsync(_gnomeshadeClient, _transactionId, Selected?.Id).Result;
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
}
