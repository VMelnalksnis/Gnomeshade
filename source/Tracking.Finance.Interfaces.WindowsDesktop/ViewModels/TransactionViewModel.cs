// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Linq;
using System.Threading.Tasks;

using Caliburn.Micro;

using Tracking.Finance.Interfaces.WebApi.Client;
using Tracking.Finance.Interfaces.WindowsDesktop.Events;
using Tracking.Finance.Interfaces.WindowsDesktop.Helpers;
using Tracking.Finance.Interfaces.WindowsDesktop.Models;

namespace Tracking.Finance.Interfaces.WindowsDesktop.ViewModels
{
	/// <summary>
	/// Overview of existing transactions.
	/// </summary>
	public sealed class TransactionViewModel : Screen, IViewModel
	{
		private readonly IEventAggregator _eventAggregator;

		/// <summary>
		/// Initializes a new instance of the <see cref="TransactionViewModel"/> class.
		/// </summary>
		/// <param name="financeClient">API client for getting transactions.</param>
		/// <param name="eventAggregator">Event aggregator for publishing events.</param>
		public TransactionViewModel(IFinanceClient financeClient, IEventAggregator eventAggregator)
		{
			_eventAggregator = eventAggregator;
			var transactions = Task.Run(financeClient.Get).Result;
			var overviews = transactions
				.Select(transaction => new TransactionOverview
				{
					Date = transaction.Date.LocalDateTime,
					Description = transaction.Description,
					SourceAccount = "foo todo",
					TargetAccount = "bar todo",
					SourceAmount = transaction.Items.Sum(item => item.SourceAmount),
					TargetAmount = transaction.Items.Sum(item => item.TargetAmount),
				});

			Overviews = new(overviews);
		}

		/// <summary>
		/// Gets a collection of transaction overviews.
		/// </summary>
		public ObservableItemCollection<TransactionOverview> Overviews { get; }

		/// <summary>
		/// Handles the Ctrl+N gesture.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		public async Task CtrlNPressed()
		{
			await _eventAggregator.PublishOnUIThreadAsync(new CreateNewTransactionEvent());
		}
	}
}
