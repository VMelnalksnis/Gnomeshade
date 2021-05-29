using System;
using System.Collections.Generic;

namespace Tracking.Finance.Web.Data.Models
{
	/// <summary>
	/// Represents an account in multiple currencies belonging to a specific <see cref="Models.FinanceUser"/>.
	/// </summary>
	public class Account : IEntity, INamedEntity, IUserSpecificEntity, IModifiableEntity, IDescribable
	{
		/// <inheritdoc/>
		public int Id { get; set; }

		/// <inheritdoc/>
		public int FinanceUserId { get; set; }

		public int? CounterpartyId { get; set; }

		/// <inheritdoc/>
		public DateTimeOffset CreatedAt { get; set; }

		/// <inheritdoc/>
		public DateTimeOffset ModifiedAt { get; set; }

		/// <inheritdoc/>
		public string Name { get; set; }

		/// <inheritdoc/>
		public string NormalizedName { get; set; }

		/// <inheritdoc/>
		public string? Description { get; set; }

		// BIC/SWIFT
		public string? Bic { get; set; }

		// IBAN
		public string? Iban { get; set; }

		public string? AccountNumber { get; set; }

		public bool Active { get; set; }

		public int PrefferedCurrencyId { get; set; }

		public bool LimitCurrencies { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="FinanceUser"/> which owns this <see cref="Account"/>.
		/// </summary>
		public FinanceUser FinanceUser { get; set; }

		public Counterparty? Counterparty { get; set; }

		public Currency PrefferedCurrency { get; set; }

		/// <summary>
		/// Gets or sets a collection of currencies which the <see cref="Account"/> can hold.
		/// </summary>
		public ICollection<AccountInCurrency> AccountsInCurrencies { get; set; }
	}
}
