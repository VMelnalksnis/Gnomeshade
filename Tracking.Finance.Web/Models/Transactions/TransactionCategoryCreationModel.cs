using System.ComponentModel.DataAnnotations;

namespace Tracking.Finance.Web.Models.Transactions
{
	public class TransactionCategoryCreationModel
	{
		[Required]
		public int? FinanceUserId { get; set; }

		[Required]
		public string Name { get; set; }
	}
}
