using System.Collections.Generic;
using System.Linq;

using Tracking.Finance.Web.Data.Models;

namespace Tracking.Finance.Web.Models
{
	public class TransactionDetailsModel
	{
		public TransactionDetailsModel(Transaction transaction, List<TransactionDetailsItemModel> itemModels)
		{
			Transaction = transaction;
			TransactionItems = itemModels;
		}

		public Transaction Transaction { get; }

		public decimal SourceAmount => TransactionItems.Select(item => item.TransactionItem.SourceAmount).Sum();

		public List<TransactionDetailsItemModel> TransactionItems { get; }
	}
}
