using System;

namespace Tracking.Finance.Web.Data.Models
{
	public class Counterparty
	{
		public int Id { get; set; }

		public int FinanceUserId { get; set; }

		public DateTimeOffset CreatedAt { get; set; }

		public DateTimeOffset ModifiedAt { get; set; }

		public string Name { get; set; }

		public string NormalizedName { get; set; }

		public FinanceUser FinanceUser { get; set; }
	}
}
