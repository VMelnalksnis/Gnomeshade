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
					TransactionCategoryId = model.TransactionCategoryId.Value,
					SourceAccountId = model.SourceAccountId.Value,
					TargetAccountId = model.TargetAccountId.Value,
					CompletedAt = model.CompletedAt.Value.Add(new TimeSpan(model.CompletedAtTime.Value.Hour, model.CompletedAtTime.Value.Minute, model.CompletedAtTime.Value.Second)),
					BankReference = model.BankReference,
					ExternalReference = model.ExternalReference,
					InternalReference = model.InternalReference,
					Description = model.Description,
					Completed = model.Completed,
					Generated = false,
					Validated = false,
				};
		}
	}
}
