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

		/// <summary>
		/// Gets or sets a value indicating whether this account can contain only a single currency.
		/// </summary>
		public bool SingleCurrency { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this account is owned by the user.
		/// </summary>
		public bool UserAccount { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="FinanceUser"/> which owns this <see cref="Account"/>.
		/// </summary>
		public FinanceUser FinanceUser { get; set; }

		/// <summary>
		/// Gets or sets a collection of currencies which the <see cref="Account"/> can hold.
		/// </summary>
		public ICollection<AccountInCurrency> AccountsInCurrencies { get; set; }
	}
}
