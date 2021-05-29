namespace Tracking.Finance.Data.Models.Abstractions
{
	/// <summary>
	/// Represents an entity.
	/// </summary>
	public interface IEntity
	{
		/// <summary>
		/// Gets the database Id of the entity.
		/// </summary>
		int Id { get; }
	}
}
