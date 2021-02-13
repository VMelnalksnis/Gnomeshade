using System;
using System.Collections.Generic;

namespace Tracking.Finance.Web.Data.Models
{
	public class Transaction
	{
		public int Id { get; set; }

		public int FinanceUserId { get; set; }

		public int TransactionCategoryId { get; set; }

		public int SourceAccountId { get; set; }

		public int TargetAccountId { get; set; }

		public DateTimeOffset CreatedAt { get; set; }

		public DateTimeOffset ModifedAt { get; set; }

		public DateTimeOffset? CompletedAt { get; set; }

		public int CounterpartyId { get; set; }

		public string BankReference { get; set; }

		public string ExternalReference { get; set; }

		public string InternalReference { get; set; }

		public string? Description { get; set; }

		public bool Generated { get; set; }

		public bool Validated { get; set; }

		public bool Completed { get; set; }

		public FinanceUser FinanceUser { get; set; }

		public TransactionCategory TransactionCategory { get; set; }

		public Account SourceAccount { get; set; }

		public Account TargetAccount { get; set; }

		public Counterparty Counterparty { get; set; }

		public ICollection<TransactionItem> TransactionItems { get; set; }
	}
}
