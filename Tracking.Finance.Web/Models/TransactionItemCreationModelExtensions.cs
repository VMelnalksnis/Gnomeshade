using Tracking.Finance.Web.Data.Models;

namespace Tracking.Finance.Web.Models
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
					SourceCurrencyId = model.SourceCurrencyId.Value,
					TargetAmount = model.TargetAmount,
					TargetCurrencyId = model.TargetCurrencyId.Value,
					Amount = model.Amount,
					ProductId = model.ProductId.Value,
				};
		}
	}
}
