using System;
using System.Collections.Generic;

namespace Tracking.Finance.Web.Data.Models
{
	/// <summary>
	/// Represents an account in multiple currencies belonging to a specific <see cref="Models.FinanceUser"/>.
	/// </summary>
	public class Account : IEntity, INamedEntity, IUserSpecificEntity, IModifiableEntity
	{
		/// <inheritdoc/>
		public int Id { get; set; }

		/// <inheritdoc/>
		public int FinanceUserId { get; set; }

		/// <inheritdoc/>
		public DateTimeOffset CreatedAt { get; set; }

		/// <inheritdoc/>
		public DateTimeOffset ModifiedAt { get; set; }

		/// <inheritdoc/>
		public string Name { get; set; }

		/// <inheritdoc/>
		public string NormalizedName { get; set; }

		public bool SingleCurrency { get; set; }

		public FinanceUser FinanceUser { get; set; }

		public ICollection<AccountInCurrency> AccountsInCurrencies { get; set; }
	}
}
