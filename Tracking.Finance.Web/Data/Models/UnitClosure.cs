namespace Tracking.Finance.Web.Data.Models
{
	/// <summary>
	/// Describes hierarchical relationship between two <see cref="Unit"/> entities.
	/// </summary>
	public class UnitClosure : IEntity
	{
		/// <inheritdoc/>
		public int Id { get; set; }

		/// <summary>
		/// Gets or sets the id of the parent <see cref="Unit"/>.
		/// </summary>
		public int ParentUnitId { get; set; }

		/// <summary>
		/// Gets or sets the id of the child <see cref="Unit"/>.
		/// </summary>
		public int ChildUnitId { get; set; }

		/// <summary>
		/// Gets or sets the depth of the hierarchy.
		/// </summary>
		public byte Depth { get; set; }

		/// <summary>
		/// Gets or sets the parent <see cref="Unit"/>.
		/// </summary>
		public Unit ParentUnit { get; set; }

		/// <summary>
		/// Gets or sets the child <see cref="Unit"/>.
		/// </summary>
		public Unit ChildUnit { get; set; }
	}
}
