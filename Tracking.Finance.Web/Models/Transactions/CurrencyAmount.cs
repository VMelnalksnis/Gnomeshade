namespace Tracking.Finance.Web.Models.Transactions
{
	public class CurrencyAmount
	{
		public decimal SourceAmount { get; set; }

		public decimal TargetAmount { get; set; }

		public string Currency { get; set; }
	}
}
