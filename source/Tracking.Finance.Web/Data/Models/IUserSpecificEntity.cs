namespace Tracking.Finance.Web.Data.Models
{
	/// <summary>
	/// Represents an entity which belongs to a specific <see cref="FinanceUser"/>.
	/// </summary>
	public interface IUserSpecificEntity
	{
		/// <summary>
		/// Gets the Id of the <see cref="FinanceUser"/> which owns this entity.
		/// </summary>
		public int FinanceUserId { get; }
	}
}
