using Tracking.Finance.Web.Data.Models;

namespace Tracking.Finance.Web.Models.Transactions
{
	public static class TransactionItemCreationModelExtensions
	{
		public static TransactionItem Map(this TransactionItemCreationModel model)
		{
			return
				new TransactionItem
				{
					FinanceUserId = model.FinanceUserId.Value,
					TransactionId = model.TransactionId.Value,
					SourceAmount = model.SourceAmount,
					SourceAccountId = model.SourceAccountId.Value,
					TargetAmount = model.TargetAmount,
					TargetAccountId = model.TargetAccountId.Value,
					Amount = model.Amount,
					ProductId = model.ProductId.Value,
				};
		}
	}
}
