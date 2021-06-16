using System;

namespace Tracking.Finance.Web.Data.Models
{
	/// <summary>
	/// Represents an <see cref="Models.Account"/> in a specific <see cref="Models.Currency"/> belonging to a <see cref="Models.FinanceUser"/>.
	/// </summary>
	public class AccountInCurrency : IEntity, IUserSpecificEntity, IModifiableEntity
	{
		/// <inheritdoc/>
		public int Id { get; set; }

		/// <summary>
		/// Gets or sets the id of the <see cref="Models.Account"/> to wchich this <see cref="AccountInCurrency"/> belongs to.
		/// </summary>
		public int AccountId { get; set; }

		/// <inheritdoc/>
		public int FinanceUserId { get; set; }

		/// <summary>
		/// Gets or sets the id of the <see cref="Models.Currency"/> which this <see cref="AccountInCurrency"/> represents for an <see cref="Models.Account"/>.
		/// </summary>
		public int CurrencyId { get; set; }

		/// <inheritdoc/>
		public DateTimeOffset CreatedAt { get; set; }

		/// <inheritdoc/>
		public DateTimeOffset ModifiedAt { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="Models.Account"/> to which this <see cref="AccountInCurrency"/> belongs to.
		/// </summary>
		public Account Account { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="FinanceUser"/> which owns this <see cref="AccountInCurrency"/>.
		/// </summary>
		public FinanceUser FinanceUser { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="Models.Currency"/> which this <see cref="AccountInCurrency"/> represents for an <see cref="Models.Account"/>.
		/// </summary>
		public Currency Currency { get; set; }
	}
}
