namespace Tracking.Finance.Web.Data.Models
{
	public class Unit
	{
		public int Id { get; set; }

		public int FinanceUserId { get; set; }

		public short Exponent { get; set; }

		public decimal Mantissa { get; set; }

		public string Name { get; set; }

		public string NormalizedName { get; set; }

		public FinanceUser FinanceUser { get; set; }
	}
}
