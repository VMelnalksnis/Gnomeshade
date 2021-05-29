using System.Collections.Generic;
using System.Linq;

using Tracking.Finance.Web.Data.Models;

namespace Tracking.Finance.Web.Models.Transactions
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

		public List<SumModel> SumModels
		{
			get
			{
				return
					TransactionItems
						.Select(item => new List<string> { item.SourceCurrency, item.TargetCurrency })
						.SelectMany(items => items)
						.Distinct()
						.Select(currency =>
						{
							var source =
								TransactionItems
									.Where(item => item.SourceCurrency == currency)
									.Sum(item => item.SourceAmount);

							var target =
								TransactionItems
									.Where(item => item.TargetCurrency == currency)
									.Sum(item => item.TargetAmount);

							return new SumModel { Currency = currency, Source = source, Target = target };
						})
						.ToList();
			}
		}
	}

	public class SumModel
	{
		public decimal Source { get; set; }

		public decimal Target { get; set; }

		public string Currency { get; set; }
	}
}
