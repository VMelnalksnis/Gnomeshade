using Tracking.Finance.Data.Models.Abstractions;

namespace Tracking.Finance.Data.Models
{
	public sealed class ProductCategory : IEntity, INamedEntity, IUserSpecificEntity, IDescribableEntity
	{
		/// <inheritdoc/>
		public int Id { get; set; }

		/// <inheritdoc/>
		public int UserId { get; set; }

		/// <inheritdoc/>
		public string Name { get; set; }

		/// <inheritdoc/>
		public string NormalizedName { get; set; }

		/// <inheritdoc/>
		public string? Description { get; set; }
	}
}
