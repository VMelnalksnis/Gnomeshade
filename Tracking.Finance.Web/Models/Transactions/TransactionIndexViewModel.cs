using System;

namespace Tracking.Finance.Web.Models.Transactions
{
	public class TransactionIndexViewModel
	{
		public TransactionIndexViewModel(
			int id,
			int sourceAccountId,
			string sourceAccountName,
			int targetAccountId,
			string targetAccountName,
			DateTime completedAt, decimal amount, string currency)
		{
			Id = id;
			SourceAccountId = sourceAccountId;
			SourceAccountName = sourceAccountName;
			TargetAccountId = targetAccountId;
			TargetAccountName = targetAccountName;
			CompletedAt = completedAt;
			Amount = amount;
			Currency = currency;
		}

		public int Id { get; }

		public int SourceAccountId { get; }

		public string SourceAccountName { get; }

		public int TargetAccountId { get; }

		public string TargetAccountName { get; }

		public DateTime CompletedAt { get; }

		public decimal Amount { get; }

		public string Currency { get; }
	}
}
