// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
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
		private readonly IFinanceClient _financeClient;

		/// <summary>
		/// Initializes a new instance of the <see cref="TransactionViewModel"/> class.
		/// </summary>
		public TransactionViewModel()
			: this(new FinanceClient())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TransactionViewModel"/> class.
		/// </summary>
		/// <param name="financeClient">Finance API client for getting finance data.</param>
		public TransactionViewModel(IFinanceClient financeClient)
		{
			_financeClient = financeClient;

			Transactions = GetTransactionsAsync();
		}

		/// <summary>
		/// Gets all transactions for the current user.
		/// </summary>
		public Task<List<TransactionOverview>> Transactions { get; }

		private async Task<List<TransactionOverview>> GetTransactionsAsync()
		{
			var transactions = await _financeClient.GetTransactionsAsync().ConfigureAwait(false);
			var overviews =
				await transactions
					.SelectAsync(async transaction =>
					{
						var firstItem = transaction.Items.FirstOrDefault();
						if (firstItem is null)
						{
							return null;
						}

						// todo don't get all accounts
						var accounts = await _financeClient.GetAccountsAsync().ConfigureAwait(false);
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
					}).ConfigureAwait(false);

			return new(overviews);
		}
	}
}
