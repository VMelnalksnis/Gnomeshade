// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Transactions;

namespace Gnomeshade.Avalonia.Core.Transactions.Transfers;

/// <summary>Overview of all <see cref="Transfer"/>s of a single <see cref="Transaction"/>.</summary>
public sealed class TransferViewModel : OverviewViewModel<TransferOverview, TransferUpsertionViewModel>
{
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly IDialogService _dialogService;
	private readonly Guid _transactionId;

	private TransferUpsertionViewModel _details;

	/// <summary>Initializes a new instance of the <see cref="TransferViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">A strongly typed API client.</param>
	/// <param name="dialogService">Service for creating dialog windows.</param>
	/// <param name="transactionId">The transaction for which to create a transfer overview.</param>
	public TransferViewModel(
		IActivityService activityService,
		IGnomeshadeClient gnomeshadeClient,
		IDialogService dialogService,
		Guid transactionId)
		: base(activityService)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_dialogService = dialogService;
		_transactionId = transactionId;
		_details = new(activityService, gnomeshadeClient, _dialogService, transactionId, null);

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

	/// <inheritdoc />
	public override async Task UpdateSelection()
	{
		Details = new(ActivityService, _gnomeshadeClient, _dialogService, _transactionId, Selected?.Id);
		await Details.RefreshAsync();
		SetDefaultCurrency(this);
	}

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		var transactionsTask = _gnomeshadeClient.GetDetailedTransactionAsync(_transactionId);
		var accountsTask = _gnomeshadeClient.GetAccountsAsync();
		await Task.WhenAll(transactionsTask, accountsTask);

		var accounts = accountsTask.Result;
		var transaction = transactionsTask.Result;
		IsReadOnly = transaction.Reconciled;

		var overviews = transaction.Transfers
			.OrderBy(transfer => transfer.Order)
			.ThenBy(transfer => transfer.ModifiedAt)
			.Select(transfer => transfer.ToOverview(accounts));

		var selected = Selected;
		var sort = DataGridView.SortDescriptions;
		Rows.CollectionChanged -= RowsOnCollectionChanged;
		Rows = new(overviews); // todo sorting
		Rows.CollectionChanged += RowsOnCollectionChanged;
		DataGridView.SortDescriptions.AddRange(sort);
		Selected = Rows.SingleOrDefault(overview => overview.Id == selected?.Id);

		if (Selected is null)
		{
			await Details.RefreshAsync();
			SetDefaultCurrency(this);
		}
	}

	/// <inheritdoc />
	protected override async Task DeleteAsync(TransferOverview row)
	{
		await _gnomeshadeClient.DeleteTransferAsync(row.Id);
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

		var lastOrder = viewModel.Rows.Select(transfer => transfer.Order).Max() ?? default;
		viewModel.Details.Order ??= lastOrder + 1;
	}

	private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(Rows))
		{
			OnPropertyChanged(nameof(Total));
		}
	}

	private void RowsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		OnPropertyChanged(nameof(Total));
	}

	private async void DetailsOnUpserted(object? sender, UpsertedEventArgs e)
	{
		await RefreshAsync();
	}
}
