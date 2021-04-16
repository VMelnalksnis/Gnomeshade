using System.Collections.Generic;

using Microsoft.AspNetCore.Identity;

namespace Tracking.Finance.Web.Data.Models
{
	public class FinanceUser : IEntity
	{
		/// <inheritdoc/>
		public int Id { get; set; }

		public string IdentityUserId { get; set; }

		public IdentityUser IdentityUser { get; set; }

		public ICollection<Account> Accounts { get; set; }

		public ICollection<Transaction> Transactions { get; set; }
	}
}
