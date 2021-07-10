// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Tracking.Finance.Interfaces.Desktop.Models;
using Tracking.Finance.Interfaces.WebApi.Client;

namespace Tracking.Finance.Interfaces.Desktop.ViewModels
{
	/// <summary>
	/// All transaction overview view model.
	/// </summary>
	public sealed class TransactionViewModel : ViewModelBase
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
						var firstItem = transaction.Items.FirstOrDefault(); // todo item should exist
						var sourceAccount = firstItem is null
							? null
							: await _financeClient.FindAccountAsync(firstItem.SourceAccountId).ConfigureAwait(false);

						var targetAccount = firstItem is null
							? null
							: await _financeClient.FindAccountAsync(firstItem.TargetAccountId).ConfigureAwait(false);

						return new TransactionOverview
						{
							Date = transaction.Date.LocalDateTime,
							Description = transaction.Description,
							SourceAccount = sourceAccount?.Name ?? string.Empty, // todo account should exist
							TargetAccount = targetAccount?.Name ?? string.Empty,
							SourceAmount = transaction.Items.Sum(item => item.SourceAmount), // todo select per currency
							TargetAmount = transaction.Items.Sum(item => item.TargetAmount),
						};
					}).ConfigureAwait(false);

			return new(overviews);
		}
	}
}
