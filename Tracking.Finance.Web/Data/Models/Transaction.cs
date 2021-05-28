using System;
using System.Collections.Generic;

namespace Tracking.Finance.Web.Data.Models
{
	public class Transaction : IEntity, IUserSpecificEntity, IModifiableEntity
	{
		/// <inheritdoc/>
		public int Id { get; set; }

		/// <inheritdoc/>
		public int FinanceUserId { get; set; }

		/// <inheritdoc/>
		public DateTimeOffset CreatedAt { get; set; }

		/// <inheritdoc/>
		public DateTimeOffset ModifiedAt { get; set; }

		public DateTimeOffset Date { get; set; }

		public string? Description { get; set; }

		public bool Generated { get; set; }

		public bool Validated { get; set; }

		public bool Completed { get; set; }

		public FinanceUser FinanceUser { get; set; }

		public ICollection<TransactionItem> TransactionItems { get; set; }
	}
}
