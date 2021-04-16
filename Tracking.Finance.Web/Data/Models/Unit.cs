namespace Tracking.Finance.Web.Data.Models
{
	public class Unit : IEntity, INamedEntity, IUserSpecificEntity
	{
		/// <inheritdoc/>
		public int Id { get; set; }

		/// <inheritdoc/>
		public int FinanceUserId { get; set; }

		public short Exponent { get; set; }

		public decimal Mantissa { get; set; }

		/// <inheritdoc/>
		public string Name { get; set; }

		/// <inheritdoc/>
		public string NormalizedName { get; set; }

		public FinanceUser FinanceUser { get; set; }
	}
}
