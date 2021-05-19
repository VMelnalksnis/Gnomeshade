using Tracking.Finance.Web.Data.Models;

namespace Tracking.Finance.Web.Models.Accounts
{
	public class AccountDetailsCurrencyViewModel
	{
		/// <summary>
		/// Gets the <see cref="Currency.AlphabeticCode"/> of the amount currency.
		/// </summary>
		public string Currency { get; init; }

		/// <summary>
		/// Gets the total incoming amount to the <see cref="Account"/>.
		/// </summary>
		public decimal In { get; init; }

		/// <summary>
		/// Gets the total outgoing amount from the <see cref="Account"/>.
		/// </summary>
		public decimal Out { get; init; }

		/// <summary>
		/// Gets the total balance of the <see cref="Account"/>.
		/// </summary>
		public decimal Balance => In - Out;
	}
}
