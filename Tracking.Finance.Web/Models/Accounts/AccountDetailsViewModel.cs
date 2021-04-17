using System.Collections.Generic;
using System.Linq;

using Tracking.Finance.Web.Data.Models;

namespace Tracking.Finance.Web.Models.Accounts
{
	public class AccountDetailsViewModel
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AccountDetailsViewModel"/> class.
		/// </summary>
		/// <param name="account">The account on which to base the detail view model.</param>
		public AccountDetailsViewModel(Account account)
		{
			Name = account.Name;
			Currencies = account.AccountsInCurrencies.ToList();
			Account = account;
		}

		public string Name { get; }

		public List<AccountInCurrency> Currencies { get; }

		public Account Account { get; }
	}
}
