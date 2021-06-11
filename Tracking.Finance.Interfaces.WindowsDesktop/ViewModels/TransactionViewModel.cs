// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Linq;
using System.Threading.Tasks;

using Caliburn.Micro;

using Tracking.Finance.Interfaces.WebApi.Client;
using Tracking.Finance.Interfaces.WindowsDesktop.Helpers;
using Tracking.Finance.Interfaces.WindowsDesktop.Models;

namespace Tracking.Finance.Interfaces.WindowsDesktop.ViewModels
{
	public sealed class TransactionViewModel : Screen, IViewModel
	{
		public TransactionViewModel(IFinanceClient financeClient)
		{
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

		public ObservableItemCollection<TransactionOverview> Overviews { get; }
	}
}
