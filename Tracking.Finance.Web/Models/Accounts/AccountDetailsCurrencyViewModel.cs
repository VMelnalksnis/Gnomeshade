using Tracking.Finance.Web.Data.Models;

namespace Tracking.Finance.Web.Models.Accounts
{
	public class AccountDetailsCurrencyViewModel
	{
		public AccountDetailsCurrencyViewModel(AccountInCurrency accountInCurrency, decimal balance)
		{
			Currency = accountInCurrency.Currency.AlphabeticCode;
			Balance = balance;
		}

		public string Currency { get; }

		public decimal Balance { get; }
	}
}
