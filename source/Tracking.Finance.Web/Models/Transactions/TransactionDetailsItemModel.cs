using System;

using Tracking.Finance.Web.Data.Models;

namespace Tracking.Finance.Web.Models.Transactions
{
	public class TransactionDetailsItemModel
	{
		public TransactionDetailsItemModel(TransactionItem transactionItem)
		{
			TransactionItem = transactionItem;
			Id = transactionItem.Id;
			SourceAmount = transactionItem.SourceAmount;
			SourceCurrency = transactionItem.SourceAccount.Currency.Name;
			TargetAmount = transactionItem.TargetAmount;
			TargetCurrency = transactionItem.TargetAccount.Currency.Name;
			Amount = transactionItem.Amount;
			Product = transactionItem.Product.Name;
			Description = transactionItem.Description;
		}

		public TransactionItem TransactionItem { get; }

		public int Id { get; }

		public decimal SourceAmount { get; }

		public string SourceCurrency { get; }

		public int SourceAccountId { get; set; }

		public string SourceAccount { get; set; }

		public decimal TargetAmount { get; set; }

		public string TargetCurrency { get; }

		public int TargetAccountId { get; set; }

		public string TargetAccount { get; set; }

		public decimal Amount { get; }

		public string Product { get; }

		public int ProductId { get; set; }

		public string? Description { get; }

		public DateTime Date { get; set; }
	}
}
