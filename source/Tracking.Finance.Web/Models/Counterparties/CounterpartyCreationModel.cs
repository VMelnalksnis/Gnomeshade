using System.ComponentModel.DataAnnotations;

namespace Tracking.Finance.Web.Models.Counterparties
{
	public class CounterpartyCreationModel
	{
		[Required]
		public int? FinanceUserId { get; set; }

		[Required]
		public string Name { get; set; }
	}
}
