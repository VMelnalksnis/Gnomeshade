namespace Tracking.Finance.Web.Models.Products
{
	public class ProductDetailModel
	{
		public ProductDetailModel(int categoryId, string category, string unit, int supplierId, string supplier, string name, string? description)
		{
			CategoryId = categoryId;
			Category = category;
			Unit = unit;
			SupplierId = supplierId;
			Supplier = supplier;
			Name = name;
			Description = description;
		}

		public int CategoryId { get; }

		public string Category { get; }

		public string Unit { get; }

		public int SupplierId { get; }

		public string Supplier { get; }

		public string Name { get; }

		public string? Description { get; }
	}
}
