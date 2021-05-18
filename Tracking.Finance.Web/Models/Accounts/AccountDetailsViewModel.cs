using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc.Rendering;

using Tracking.Finance.Web.Data.Models;

namespace Tracking.Finance.Web.Models.Accounts
{
	public class AccountDetailsViewModel
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AccountDetailsViewModel"/> class.
		/// </summary>
		/// <param name="account">The account on which to base the detail view model.</param>
		public AccountDetailsViewModel(Account account, List<AccountDetailsCurrencyViewModel> accountsInCurrencies, List<SelectListItem> currencies)
		{
			Id = account.Id;
			Name = account.Name;
			CurrencyListItems = currencies;
			Currencies = accountsInCurrencies;
		}

		public int Id { get; }

		public string Name { get; }

		public List<SelectListItem> CurrencyListItems { get; }

		public List<AccountDetailsCurrencyViewModel> Currencies { get; }
	}
}
