// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Collections;
using Avalonia.Input;

using Gnomeshade.Interfaces.WebApi.Client;

namespace Gnomeshade.Interfaces.Avalonia.Core.Transactions.Items;

/// <summary>All transaction overview view model.</summary>
public sealed class TransactionViewModel : ViewModelBase
{
	private readonly IGnomeshadeClient _gnomeshadeClient;

	private DateTimeOffset _from;
	private DateTimeOffset _to;
	private DataGridItemCollectionView<TransactionOverview> _dataGridView = new(new List<TransactionOverview>(0));
	private TransactionOverview? _selectedOverview;
	private TransactionDetailViewModel? _transaction;

	private TransactionViewModel(
		IGnomeshadeClient gnomeshadeClient,
		DateTimeOffset from,
		DateTimeOffset to,
		DataGridItemCollectionView<TransactionOverview> dataGridView)
	{
		_gnomeshadeClient = gnomeshadeClient;
		From = from;
		To = to;
		Transactions = dataGridView;

		PropertyChanged += OnPropertyChanged;
	}

	/// <summary>Raised when a transaction is selected for viewing its details.</summary>
	public event EventHandler<TransactionSelectedEventArgs>? TransactionSelected;

	/// <summary>Gets or sets the time from which to select the transactions.</summary>
	public DateTimeOffset From
	{
		get => _from;
		set => SetAndNotify(ref _from, value, nameof(From));
	}

	/// <summary>Gets or sets the time until which to select the transactions.</summary>
	public DateTimeOffset To
	{
		get => _to;
		set => SetAndNotify(ref _to, value, nameof(To));
	}

	/// <summary>Gets the grid view of all transactions.</summary>
	public DataGridCollectionView DataGridView => Transactions;

	/// <summary>Gets a typed collection of all transactions for the current user.</summary>
	public DataGridItemCollectionView<TransactionOverview> Transactions
	{
		get => _dataGridView;
		private set
		{
			Transactions.CollectionChanged -= DataGridViewOnCollectionChanged;
			SetAndNotifyWithGuard(ref _dataGridView, value, nameof(Transactions), nameof(DataGridView));
			Transactions.CollectionChanged += DataGridViewOnCollectionChanged;
			var sortDescriptions = new DataGridSortDescriptionCollection
			{
				DataGridSortDescription.FromPath(nameof(TransactionOverview.BookedAt)),
				DataGridSortDescription.FromPath(nameof(TransactionOverview.ValuedAt)),
			};
			DataGridView.SortDescriptions.AddRange(sortDescriptions);
		}
	}

	/// <summary>Gets or sets the selected row in <see cref="DataGridView"/>.</summary>
	public TransactionOverview? SelectedOverview
	{
		get => _selectedOverview;
		set => SetAndNotifyWithGuard(ref _selectedOverview, value, nameof(SelectedOverview), nameof(CanDelete));
	}

	/// <summary>Gets the transaction detail view of the <see cref="SelectedOverview"/>.</summary>
	public TransactionDetailViewModel? Transaction
	{
		get => _transaction;
		private set => SetAndNotify(ref _transaction, value);
	}

	/// <summary>Gets a value indicating whether the selected transaction can be deleted.</summary>
	public bool CanDelete => SelectedOverview is not null;

	/// <summary>Asynchronously creates a new instance of the <see cref="TransactionViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	/// <returns>A new instance of the <see cref="TransactionViewModel"/> class.</returns>
	public static async Task<TransactionViewModel> CreateAsync(IGnomeshadeClient gnomeshadeClient)
	{
		var to = DateTimeOffset.Now;
		var from = new DateTimeOffset(to.Year, to.Month, 01, 0, 0, 0, to.Offset);

		var transactions = await CreateDataGridViewAsync(gnomeshadeClient, from, to);
		return new(gnomeshadeClient, from, to, transactions);
	}

	/// <summary>Handles the <see cref="InputElement.DoubleTapped"/> event for <see cref="DataGridView"/>.</summary>
	public void OnDataGridDoubleTapped()
	{
		if (SelectedOverview is null || TransactionSelected is null)
		{
			return;
		}

		TransactionSelected(this, new(SelectedOverview.Id));
	}

	/// <summary>Deletes all the selected transactions.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task DeleteSelectedAsync()
	{
		if (SelectedOverview is null)
		{
			throw new InvalidOperationException();
		}

		await _gnomeshadeClient.DeleteTransactionAsync(SelectedOverview.Id).ConfigureAwait(false);
		await SearchAsync().ConfigureAwait(false);
	}

	/// <summary>Searches for transaction using the specified filters.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task SearchAsync()
	{
		Transactions = await CreateDataGridViewAsync(_gnomeshadeClient, From, To).ConfigureAwait(false);
	}

	private static async Task<DataGridItemCollectionView<TransactionOverview>> CreateDataGridViewAsync(
		IGnomeshadeClient gnomeshadeClient,
		DateTimeOffset? from,
		DateTimeOffset? to)
	{
		// todo don't get all accounts
		var accounts = await gnomeshadeClient.GetAccountsAsync().ConfigureAwait(false);
		var transactions = await gnomeshadeClient.GetTransactionsAsync(from, to).ConfigureAwait(false);
		var counterparties = await gnomeshadeClient.GetCounterpartiesAsync().ConfigureAwait(false);
		var userCounterparty = await gnomeshadeClient.GetMyCounterpartyAsync().ConfigureAwait(false);

		var transactionOverviews = transactions.Translate(accounts, counterparties, userCounterparty).ToList();
		return new(transactionOverviews);
	}

	private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is not nameof(SelectedOverview))
		{
			return;
		}

		if (SelectedOverview is null)
		{
			Transaction = null;
			return;
		}

		Transaction = Task
			.Run(() => TransactionDetailViewModel.CreateAsync(_gnomeshadeClient, SelectedOverview.Id))
			.ConfigureAwait(false)
			.GetAwaiter()
			.GetResult();
	}

	private void DataGridViewOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		OnPropertyChanged(nameof(DataGridView));
		OnPropertyChanged(nameof(CanDelete));
	}
}
