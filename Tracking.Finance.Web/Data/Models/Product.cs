using System;

namespace Tracking.Finance.Web.Data.Models
{
	public class Product
	{
		public int Id { get; set; }

		public int FinanceUserId { get; set; }

		public int ProductCategoryId { get; set; }

		public int UnitId { get; set; }

		public int SupplierId { get; set; }

		public DateTimeOffset CreatedAt { get; set; }

		public DateTimeOffset ModifiedAt { get; set; }

		public string Name { get; set; }

		public string NormalizedName { get; set; }

		public string? Description { get; set; }

		public FinanceUser FinanceUser { get; set; }

		public ProductCategory ProductCategory { get; set; }

		public Unit Unit { get; set; }
	}
}
