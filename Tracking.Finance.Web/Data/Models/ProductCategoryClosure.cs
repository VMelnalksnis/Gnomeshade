namespace Tracking.Finance.Web.Data.Models
{
	public class ProductCategoryClosure
	{
		public int Id { get; set; }

		public int ParentProductCategoryId { get; set; }

		public int ChildProductCategoryId { get; set; }

		public byte Depth { get; set; }

		public ProductCategory ParentProductCategory { get; set; }

		public ProductCategory ChildProductCategory { get; set; }
	}
}
