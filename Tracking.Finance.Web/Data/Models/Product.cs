using System;

namespace Tracking.Finance.Web.Data.Models
{
	/// <summary>
	/// Represents a product that was exchanged in a <see cref="TransactionItem"/>.
	/// </summary>
	public class Product : IEntity, INamedEntity, IUserSpecificEntity, IModifiableEntity, IDescribable
	{
		/// <inheritdoc/>
		public int Id { get; set; }

		/// <inheritdoc/>
		public int FinanceUserId { get; set; }

		/// <summary>
		/// Gets or sets the id of the <see cref="ProductCategory"/> to which this <see cref="Product"/> belongs to.
		/// </summary>
		public int ProductCategoryId { get; set; }

		/// <summary>
		/// Gets or sets the id of the <see cref="Unit"/> in which the amount of this <see cref="Product"/> is expressed in.
		/// </summary>
		public int UnitId { get; set; }

		/// <summary>
		/// Gets or sets the id of the <see cref="Counterparty"/> whichs supplies/manufactures this <see cref="Product"/>.
		/// </summary>
		public int SupplierId { get; set; }

		/// <inheritdoc/>
		public DateTimeOffset CreatedAt { get; set; }

		/// <inheritdoc/>
		public DateTimeOffset ModifiedAt { get; set; }

		/// <inheritdoc/>
		public string Name { get; set; } = null!;

		/// <inheritdoc/>
		public string NormalizedName { get; set; } = null!;

		/// <inheritdoc/>
		public string? Description { get; set; }

		/// <summary>
		/// Gets or sets the user who owns this <see cref="Product"/>.
		/// </summary>
		public FinanceUser FinanceUser { get; set; } = null!;

		/// <summary>
		/// Gets or sets the product category to which this <see cref="Product"/> belongs to.
		/// </summary>
		public ProductCategory ProductCategory { get; set; } = null!;

		/// <summary>
		/// Gets or sets the <see cref="Unit"/> in which the amount of this <see cref="Product"/> is expressed in.
		/// </summary>
		public Unit Unit { get; set; } = null!;

		/// <summary>
		/// Gets or sets the <see cref="Counterparty"/> whichs supplies/manufactures this <see cref="Product"/>.
		/// </summary>
		public Counterparty Supplier { get; set; } = null!;
	}
}
