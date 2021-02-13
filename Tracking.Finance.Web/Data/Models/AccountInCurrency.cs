using System;

namespace Tracking.Finance.Web.Data.Models
{
	/// <summary>
	/// Represents an <see cref="Models.Account"/> in a specific <see cref="Models.Currency"/> belonging to a <see cref="Models.FinanceUser"/>.
	/// </summary>
	public class AccountInCurrency
	{
		public int Id { get; set; }

		public int AccountId { get; set; }

		public int FinanceUserId { get; set; }

		public int CurrencyId { get; set; }

		public DateTimeOffset CreatedAt { get; set; }

		public DateTimeOffset ModifedAt { get; set; }

		public Account Account { get; set; }

		public FinanceUser FinanceUser { get; set; }

		public Currency Currency { get; set; }
	}
}
