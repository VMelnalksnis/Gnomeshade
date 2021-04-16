using System;

namespace Tracking.Finance.Web.Data.Models
{
	public class Product : IEntity, INamedEntity, IUserSpecificEntity, IModifiableEntity
	{
		/// <inheritdoc/>
		public int Id { get; set; }

		/// <inheritdoc/>
		public int FinanceUserId { get; set; }

		public int ProductCategoryId { get; set; }

		public int UnitId { get; set; }

		public int SupplierId { get; set; }

		/// <inheritdoc/>
		public DateTimeOffset CreatedAt { get; set; }

		/// <inheritdoc/>
		public DateTimeOffset ModifiedAt { get; set; }

		/// <inheritdoc/>
		public string Name { get; set; }

		/// <inheritdoc/>
		public string NormalizedName { get; set; }

		public string? Description { get; set; }

		public FinanceUser FinanceUser { get; set; }

		public ProductCategory ProductCategory { get; set; }

		public Unit Unit { get; set; }
	}
}
