// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.Desktop.Models;
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
		private Task<ObservableCollection<TransactionOverview>> _transactions;

		/// <summary>
		/// Initializes a new instance of the <see cref="TransactionViewModel"/> class.
		/// </summary>
		public TransactionViewModel()
			: this(new GnomeshadeClient())
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
			_transactions = GetTransactionsAsync(From, To);
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
		/// Gets all transactions for the current user.
		/// </summary>
		public Task<ObservableCollection<TransactionOverview>> Transactions
		{
			get => _transactions;
			private set => SetAndNotify(ref _transactions, value, nameof(Transactions));
		}

		/// <summary>
		/// Searches for transaction using the specified filters.
		/// </summary>
		public void Search()
		{
			Transactions = GetTransactionsAsync(From, To);
		}

		private async Task<ObservableCollection<TransactionOverview>> GetTransactionsAsync(
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
						var sourceAccount = accounts.Single(account => account.Currencies.Any(currency => currency.Id == firstItem.SourceAccountId));
						var targetAccount = accounts.Single(account => account.Currencies.Any(currency => currency.Id == firstItem.TargetAccountId));

						return new TransactionOverview
						{
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
