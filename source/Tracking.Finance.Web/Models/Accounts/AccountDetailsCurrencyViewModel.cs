using Tracking.Finance.Web.Data.Models;

namespace Tracking.Finance.Web.Models.Accounts
{
	/// <summary>
	/// Information about a specific <see cref="Account"/> and <see cref="Currency"/>.
	/// </summary>
	public record AccountDetailsCurrencyViewModel(string Currency, decimal In, decimal Out)
	{
		/// <summary>
		/// Gets the <see cref="Currency.AlphabeticCode"/> of the amount currency.
		/// </summary>
		public string Currency { get; init; } = Currency;

		/// <summary>
		/// Gets the total incoming amount to the <see cref="Account"/>.
		/// </summary>
		public decimal In { get; init; } = In;

		/// <summary>
		/// Gets the total outgoing amount from the <see cref="Account"/>.
		/// </summary>
		public decimal Out { get; init; } = Out;

		/// <summary>
		/// Gets the total balance of the <see cref="Account"/>.
		/// </summary>
		public decimal Balance => In - Out;
	}
}
