namespace Tracking.Finance.Web.Data.Models
{
	/// <summary>
	/// Represents an entity which has a name.
	/// </summary>
	public interface INamedEntity
	{
		/// <summary>
		/// Gets or sets the name of the entity.
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// Gets or sets the normalized name of the entity which should be used for comparison.
		/// </summary>
		string NormalizedName { get; set; }
	}
}
