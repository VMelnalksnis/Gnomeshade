using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc.Rendering;

namespace Tracking.Finance.Web.Models.Accounts
{
	public class AccountDetailsViewModel
	{
		public int Id { get; init; }

		public string Name { get; init; }

		public bool SingleCurrency { get; init; }

		public List<SelectListItem> CurrencyListItems { get; init; }

		public List<AccountDetailsCurrencyViewModel> Currencies { get; init; }
	}
}
