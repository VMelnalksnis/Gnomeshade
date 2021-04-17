using System.Collections.Generic;

using Tracking.Finance.Web.Data.Models;

namespace Tracking.Finance.Web.Models.Accounts
{
	public class AccountIndexViewModel
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AccountIndexViewModel"/> class.
		/// </summary>
		public AccountIndexViewModel(List<Account> accounts)
		{
			Accounts = accounts;
		}

		public List<Account> Accounts { get; set; }
	}
}
