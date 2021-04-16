namespace Tracking.Finance.Web.Data.Models
{
	/// <summary>
	/// Represents an entity which has a name.
	/// </summary>
	public interface INamedEntity
	{
		/// <summary>
		/// Gets the name of the entity.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets the normalized name of the entity which should be used for comparison.
		/// </summary>
		string NormalizedName { get; }
	}
}
