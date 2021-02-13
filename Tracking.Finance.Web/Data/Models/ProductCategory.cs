namespace Tracking.Finance.Web.Data.Models
{
	public class ProductCategory
	{
		public int Id { get; set; }

		public int FinanceUserId { get; set; }

		public string Name { get; set; }

		public string NormalizedName { get; set; }

		public string? Description { get; set; }

		public FinanceUser FinanceUser { get; set; }
	}
}
