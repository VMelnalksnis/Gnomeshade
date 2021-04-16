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

		public int AccountId { get; set; }

		/// <inheritdoc/>
		public int FinanceUserId { get; set; }

		public int CurrencyId { get; set; }

		/// <inheritdoc/>
		public DateTimeOffset CreatedAt { get; set; }

		/// <inheritdoc/>
		public DateTimeOffset ModifiedAt { get; set; }

		public Account Account { get; set; }

		public FinanceUser FinanceUser { get; set; }

		public Currency Currency { get; set; }
	}
}
