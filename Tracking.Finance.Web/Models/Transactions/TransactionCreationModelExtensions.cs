using System;

using Tracking.Finance.Web.Data.Models;

namespace Tracking.Finance.Web.Models.Transactions
{
	public static class TransactionCreationModelExtensions
	{
		public static Transaction Map(this TransactionCreationModel model)
		{
			return
				new Transaction
				{
					FinanceUserId = model.FinanceUserId.Value,
					Date = model.Date.Value.Add(new TimeSpan(model.Time.Value.Hour, model.Time.Value.Minute, model.Time.Value.Second)),
					Description = model.Description,
					Completed = model.Completed,
					Generated = false,
					Validated = false,
				};
		}
	}
}
