using Tracking.Finance.Web.Data.Models;

namespace Tracking.Finance.Web.Models.Accounts
{
	/// <summary>
	/// General information about an <see cref="Account"/>.
	/// </summary>
	public record AccountIndexViewModel(int Id, string Name, bool UserAccount)
	{
		/// <summary>
		/// Gets the id of the <see cref="Account"/>.
		/// </summary>
		public int Id { get; init; } = Id;

		/// <summary>
		/// Gets the name of the <see cref="Account"/>.
		/// </summary>
		public string Name { get; init; } = Name;

		/// <summary>
		/// Gets a value indicating whether the <see cref="Account"/> is owned by the user.
		/// </summary>
		public bool UserAccount { get; init; } = UserAccount;
	}
}
