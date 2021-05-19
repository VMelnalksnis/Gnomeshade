using Tracking.Finance.Web.Data.Models;

namespace Tracking.Finance.Web.Models.Accounts
{
	public class AccountDetailsCurrencyViewModel
	{
		public AccountDetailsCurrencyViewModel(AccountInCurrency accountInCurrency, decimal @in, decimal @out, decimal balance)
		{
			Currency = accountInCurrency.Currency.AlphabeticCode;
			In = @in;
			Out = @out;
			Balance = balance;
		}

		public string Currency { get; }

		public decimal In { get; }

		public decimal Out { get; }

		public decimal Balance { get; }
	}
}
