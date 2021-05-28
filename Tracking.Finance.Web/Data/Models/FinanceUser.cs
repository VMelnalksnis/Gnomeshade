using System.Collections.Generic;

using Microsoft.AspNetCore.Identity;

namespace Tracking.Finance.Web.Data.Models
{
	public class FinanceUser : IEntity
	{
		/// <inheritdoc/>
		public int Id { get; set; }

		public string IdentityUserId { get; set; }

		public int? CounterpartyId { get; set; }

		public IdentityUser IdentityUser { get; set; }

		public Counterparty? Counterparty { get; set; }

		public ICollection<Account> Accounts { get; set; }

		public ICollection<AccountInCurrency> AccountsInCurrency { get; set; }

		public ICollection<Counterparty> Counterparties { get; set; }

		public ICollection<Product> Products { get; set; }

		public ICollection<ProductCategory> ProductCategories { get; set; }

		public ICollection<ProductCategoryClosure> ProductCategoryClosures { get; set; }

		public ICollection<Transaction> Transactions { get; set; }

		public ICollection<TransactionItem> TransactionItems { get; set; }

		public ICollection<Unit> Units { get; set; }
	}
}
