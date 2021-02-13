using System;
using System.Collections.Generic;

namespace Tracking.Finance.Web.Data.Models
{
	/// <summary>
	/// Represents an account in multiple currencies belonging to a specific <see cref="Models.FinanceUser"/>.
	/// </summary>
	public class Account
	{
		public int Id { get; set; }

		public int FinanceUserId { get; set; }

		public DateTimeOffset CreatedAt { get; set; }

		public DateTimeOffset ModifiedAt { get; set; }

		public string Name { get; set; }

		public string NormalizedName { get; set; }

		public bool SingleCurrency { get; set; }

		public FinanceUser FinanceUser { get; set; }

		public ICollection<AccountInCurrency> AccountsInCurrencies { get; set; }
	}
}
