﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Collections;
using Avalonia.Controls;

using Gnomeshade.Interfaces.Desktop.Models;
using Gnomeshade.Interfaces.Desktop.ViewModels.Binding;
using Gnomeshade.Interfaces.Desktop.ViewModels.Design;
using Gnomeshade.Interfaces.Desktop.ViewModels.Events;
using Gnomeshade.Interfaces.Desktop.Views;
using Gnomeshade.Interfaces.WebApi.Client;

namespace Gnomeshade.Interfaces.Desktop.ViewModels
{
	/// <summary>
	/// All transaction overview view model.
	/// </summary>
	public sealed class TransactionViewModel : ViewModelBase<TransactionView>
	{
		private readonly IGnomeshadeClient _gnomeshadeClient;

		private DateTimeOffset _from;
		private DateTimeOffset _to;
		private DataGridItemCollectionView<TransactionOverview> _dataGridView = new(new List<TransactionOverview>(0));
		private bool _selectAll;
		private TransactionOverview? _selectedOverview;

		/// <summary>
		/// Initializes a new instance of the <see cref="TransactionViewModel"/> class.
		/// </summary>
		public TransactionViewModel()
			: this(new DesignTimeGnomeshadeClient())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TransactionViewModel"/> class.
		/// </summary>
		/// <param name="gnomeshadeClient">API client for getting finance data.</param>
		public TransactionViewModel(IGnomeshadeClient gnomeshadeClient)
		{
			_gnomeshadeClient = gnomeshadeClient;

			To = DateTimeOffset.Now;
			From = new(To.Year, _to.Month, 01, 0, 0, 0, To.Offset);
			SearchAsync().GetAwaiter().GetResult();
		}

		/// <summary>
		/// Raised when a transaction is selected for viewing its details.
		/// </summary>
		public event EventHandler<TransactionSelectedEventArgs>? TransactionSelected;

		/// <summary>
		/// Gets or sets the time from which to select the transactions.
		/// </summary>
		public DateTimeOffset From
		{
			get => _from;
			set => SetAndNotify(ref _from, value, nameof(From));
		}

		/// <summary>
		/// Gets or sets the time until which to select the transactions.
		/// </summary>
		public DateTimeOffset To
		{
			get => _to;
			set => SetAndNotify(ref _to, value, nameof(To));
		}

		/// <summary>
		/// Gets or sets a value indicating whether all transactions are selected.
		/// </summary>
		public bool SelectAll
		{
			get => _selectAll;
			set
			{
				foreach (var transactionOverview in Transactions)
				{
					transactionOverview.Selected = value;
				}

				SetAndNotify(ref _selectAll, value, nameof(SelectAll));
			}
		}

		/// <summary>
		/// Gets the grid view of all transactions.
		/// </summary>
		public DataGridCollectionView DataGridView => Transactions;

		/// <summary>
		/// Gets a typed collection of all transactions for the current user.
		/// </summary>
		public DataGridItemCollectionView<TransactionOverview> Transactions
		{
			get => _dataGridView;
			private set
			{
				Transactions.CollectionChanged -= DataGridViewOnCollectionChanged;
				SetAndNotifyWithGuard(ref _dataGridView, value, nameof(Transactions), nameof(DataGridView));
				Transactions.CollectionChanged += DataGridViewOnCollectionChanged;
			}
		}

		/// <summary>
		/// Gets or sets the selected row in <see cref="DataGridView"/>.
		/// </summary>
		public TransactionOverview? SelectedOverview
		{
			get => _selectedOverview;
			set => SetAndNotify(ref _selectedOverview, value, nameof(SelectedOverview));
		}

		/// <summary>
		/// Gets a value indicating whether the selected transaction can be deleted.
		/// </summary>
		public bool CanDelete => Transactions.Any(transaction => transaction.Selected);

		/// <summary>
		/// Handles the <see cref="DataGrid.DoubleTapped"/> event for <see cref="DataGridView"/>.
		/// </summary>
		public void OnDataGridDoubleTapped()
		{
			if (SelectedOverview is null || TransactionSelected is null)
			{
				return;
			}

			TransactionSelected(this, new(SelectedOverview.Id));
		}

		/// <summary>
		/// Deletes all the selected transactions.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		public async Task DeleteSelectedAsync()
		{
			var selectedIds =
				Transactions
					.Where(transaction => transaction.Selected)
					.Select(transaction => transaction.Id)
					.Distinct();

			foreach (var id in selectedIds)
			{
				await _gnomeshadeClient.DeleteTransactionAsync(id).ConfigureAwait(false);
			}

			await SearchAsync().ConfigureAwait(false);
		}

		/// <summary>
		/// Searches for transaction using the specified filters.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		public async Task SearchAsync()
		{
			Transactions = await CreateDataGridViewAsync(From, To).ConfigureAwait(false);
			SelectAll = false; // todo this probably is pretty bad performance wise
		}

		private async Task<DataGridItemCollectionView<TransactionOverview>> CreateDataGridViewAsync(
			DateTimeOffset? from,
			DateTimeOffset? to)
		{
			// todo don't get all accounts
			var accounts = await _gnomeshadeClient.GetAccountsAsync().ConfigureAwait(false);
			var transactions = await _gnomeshadeClient.GetTransactionsAsync(from, to).ConfigureAwait(false);

			var transactionOverviews = transactions.Translate(accounts).ToList();
			return new(transactionOverviews);
		}

		private void DataGridViewOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			OnPropertyChanged(nameof(DataGridView));
			OnPropertyChanged(nameof(CanDelete));
		}
	}
}
