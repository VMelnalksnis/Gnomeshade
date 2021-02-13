using System;

namespace Tracking.Finance.Web.Data.Models
{
	public class TransactionItem
	{
		public int Id { get; set; }

		public int FinanceUserId { get; set; }

		public int TransactionId { get; set; }

		public decimal SourceAmount { get; set; }

		public int SourceCurrencyId { get; set; }

		public decimal TargetAmount { get; set; }

		public int TargetCurrencyId { get; set; }

		public DateTimeOffset CreatedAt { get; set; }

		public DateTimeOffset ModifiedAt { get; set; }

		public int ProductId { get; set; }

		public decimal Amount { get; set; }

		public string? Description { get; set; }

		public FinanceUser FinanceUser { get; set; }

		public Transaction Transaction { get; set; }

		public Currency SourceCurrency { get; set; }

		public Currency TargetCurrency { get; set; }

		public Product Product { get; set; }
	}
}
