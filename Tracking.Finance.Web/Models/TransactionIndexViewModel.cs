using System.Collections.Generic;

using Tracking.Finance.Web.Data.Models;

namespace Tracking.Finance.Web.Models
{
	public class TransactionIndexViewModel
	{
		public TransactionIndexViewModel(List<Transaction> transactions)
		{
			Transactions = transactions;
		}

		public List<Transaction> Transactions { get; }
	}
}
