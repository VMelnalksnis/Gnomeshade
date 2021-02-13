using System.Collections.Generic;

namespace Tracking.Finance.Web.Data.Models
{
	public class TransactionCategory
	{
		public int Id { get; set; }

		public int FinanceUserId { get; set; }

		public string Name { get; set; }

		public string NormalizedName { get; set; }

		public FinanceUser FinanceUser { get; set; }

		public ICollection<Transaction> Transactions { get; set; }
	}
}
