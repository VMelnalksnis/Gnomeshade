namespace Tracking.Finance.Web.Data.Models
{
	public class ProductCategory : IEntity, INamedEntity, IUserSpecificEntity
	{
		/// <inheritdoc/>
		public int Id { get; set; }

		/// <inheritdoc/>
		public int FinanceUserId { get; set; }

		/// <inheritdoc/>
		public string Name { get; set; }

		/// <inheritdoc/>
		public string NormalizedName { get; set; }

		public string? Description { get; set; }

		public FinanceUser FinanceUser { get; set; }
	}
}
