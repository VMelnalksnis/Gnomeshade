// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Collections;

using Gnomeshade.Interfaces.Desktop.Models;
using Gnomeshade.Interfaces.Desktop.ViewModels.Design;
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
		private DataGridCollectionView _dataGridView;
		private bool _selectAll;

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
			_dataGridView = CreateDataGridViewAsync(From, To).GetAwaiter().GetResult();
		}

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
		/// Gets all transactions for the current user.
		/// </summary>
		/// <remarks>
		/// Collection item data binding to source only works when using <see cref="DataGridCollectionView"/>.
		/// Otherwise, it would be nice to use a generic collection and remove <see cref="Transactions"/>.
		/// </remarks>
		public DataGridCollectionView DataGridView
		{
			get => _dataGridView;
			private set => SetAndNotify(ref _dataGridView, value, nameof(DataGridView));
		}

		/// <summary>
		/// Gets a value indicating whether the selected transaction can be deleted.
		/// </summary>
		public bool CanDelete => DataGridView.Cast<TransactionOverview>().Any(transaction => transaction.Selected);

		/// <summary>
		/// Gets a typed collection of transaction in <see cref="DataGridView"/>.
		/// </summary>
		public IEnumerable<TransactionOverview> Transactions => DataGridView.Cast<TransactionOverview>();

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
			DataGridView = await CreateDataGridViewAsync(From, To).ConfigureAwait(false);
			SelectAll = false; // todo this probably is pretty bad performance wise
		}

		private async Task<DataGridCollectionView> CreateDataGridViewAsync(
			DateTimeOffset? from,
			DateTimeOffset? to)
		{
			// todo don't get all accounts
			var accounts = await _gnomeshadeClient.GetAccountsAsync().ConfigureAwait(false);
			var transactions = await _gnomeshadeClient.GetTransactionsAsync(from, to).ConfigureAwait(false);

			var overviews =
				transactions
					.Select(transaction =>
					{
						var firstItem = transaction.Items.First();
						var sourceAccount = accounts.Single(account =>
							account.Currencies.Any(currency => currency.Id == firstItem.SourceAccountId));
						var targetAccount = accounts.Single(account =>
							account.Currencies.Any(currency => currency.Id == firstItem.TargetAccountId));

						return new TransactionOverview
						{
							Id = transaction.Id,
							Date = transaction.Date.LocalDateTime,
							Description = transaction.Description,
							SourceAccount = sourceAccount.Name,
							TargetAccount = targetAccount.Name,
							SourceAmount = transaction.Items.Sum(item => item.SourceAmount), // todo select per currency
							TargetAmount = transaction.Items.Sum(item => item.TargetAmount),
						};
					});

			return new(overviews);
		}
	}
}
