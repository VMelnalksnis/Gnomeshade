using System.Collections.Generic;

using Microsoft.AspNetCore.Identity;

namespace Tracking.Finance.Web.Data.Models
{
	public class FinanceUser
	{
		public int Id { get; set; }

		public string IdentityUserId { get; set; }

		public IdentityUser IdentityUser { get; set; }

		public ICollection<Account> Accounts { get; set; }
	}
}
