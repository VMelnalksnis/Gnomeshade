// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Linq;
using System.Threading.Tasks;

using Tracking.Finance.Interfaces.Desktop.Models;
using Tracking.Finance.Interfaces.Desktop.ViewModels.Observable;
using Tracking.Finance.Interfaces.WebApi.Client;

namespace Tracking.Finance.Interfaces.Desktop.ViewModels
{
	public sealed class TransactionViewModel : ViewModelBase
	{
		private readonly MainWindowViewModel _mainWindow;
		private readonly IFinanceClient _financeClient;

		/// <summary>
		/// Initializes a new instance of the <see cref="TransactionViewModel"/> class.
		/// </summary>
		public TransactionViewModel()
			: this(new(), new FinanceClient())
		{
		}

		public TransactionViewModel(MainWindowViewModel mainWindow, IFinanceClient financeClient)
		{
			_mainWindow = mainWindow;
			_financeClient = financeClient;

			Transactions = GetTransactionsAsync();
		}

		public Task<ObservableItemCollection<TransactionOverview>> Transactions { get; }

		private async Task<ObservableItemCollection<TransactionOverview>> GetTransactionsAsync()
		{
			var transactions = await _financeClient.GetTransactionsAsync().ConfigureAwait(false);
			var overviews =
				transactions
					.Select(transaction => new TransactionOverview
					{
						Date = transaction.Date.LocalDateTime,
						Description = transaction.Description,
						SourceAccount = transaction.Items.FirstOrDefault()?.SourceAccountId.ToString(),
						TargetAccount = transaction.Items.FirstOrDefault()?.TargetAccountId.ToString(),
						SourceAmount = transaction.Items.Sum(item => item.SourceAmount),
						TargetAmount = transaction.Items.Sum(item => item.TargetAmount),
					})
					.ToList();

			return new(overviews);
		}
	}
}
