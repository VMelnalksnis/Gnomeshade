using System;

namespace Tracking.Finance.Web.Data.Models
{
	/// <summary>
	/// A unit for describing the <see cref="Product"/> amount in <see cref="TransactionItem"/>.
	/// </summary>
	/// <seealso cref="UnitClosure"/>.
	public class Unit : IEntity, INamedEntity, IUserSpecificEntity, IModifiableEntity
	{
		/// <inheritdoc/>
		public int Id { get; set; }

		/// <inheritdoc/>
		public int FinanceUserId { get; set; }

		/// <summary>
		/// Gets or sets the exponent of the scaling multiplier from the parent unit.
		/// </summary>
		public short Exponent { get; set; }

		/// <summary>
		/// Gets or sets the mantissa of the scaling multiplier from the parent unit.
		/// </summary>
		public decimal Mantissa { get; set; }

		/// <inheritdoc/>
		public string Name { get; set; }

		/// <inheritdoc/>
		public string NormalizedName { get; set; }

		/// <inheritdoc/>
		public DateTimeOffset CreatedAt { get; set; }

		/// <inheritdoc/>
		public DateTimeOffset ModifiedAt { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="Models.FinanceUser"/> to which the <see cref="Unit"/> belongs to.
		/// </summary>
		public FinanceUser FinanceUser { get; set; }
	}
}
