using System.Collections.Generic;

using Tracking.Finance.Web.Data.Models;

namespace Tracking.Finance.Web.Models
{
	public class AccountsIndexViewModel
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AccountsIndexViewModel"/> class.
		/// </summary>
		public AccountsIndexViewModel(List<Account> accounts)
		{
			Accounts = accounts;
		}

		public List<Account> Accounts { get; set; }
	}
}
