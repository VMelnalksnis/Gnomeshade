namespace Tracking.Finance.Data.Models.Abstractions
{
	public interface IDescribableEntity
	{
		/// <summary>
		/// Gets the description of this entity.
		/// </summary>
		string? Description { get; }
	}
}
