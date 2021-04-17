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
			SourceCurrency = transactionItem.SourceCurrency.Name;
			TargetAmount = transactionItem.TargetAmount;
			TargetCurrency = transactionItem.TargetCurrency.Name;
			Amount = transactionItem.Amount;
			Product = transactionItem.Product.Name;
			Description = transactionItem.Description;
		}

		public TransactionItem TransactionItem { get; }

		public int Id { get; }

		public decimal SourceAmount { get; }

		public string SourceCurrency { get; }

		public decimal TargetAmount { get; }

		public string TargetCurrency { get; }

		public decimal Amount { get; }

		public string Product { get; }

		public string? Description { get; }
	}
}
