using Tracking.Finance.Data.Models.Abstractions;

namespace Tracking.Finance.Data.Models
{
	public sealed class ProductCategoryClosure : IEntity
	{
		/// <inheritdoc/>
		public int Id { get; set; }

		public int ParentProductCategoryId { get; set; }

		public int ChildProductCategoryId { get; set; }

		public byte Depth { get; set; }
	}
}
