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

		public int TransactionCategoryId { get; set; }

		public int SourceAccountId { get; set; }

		public int TargetAccountId { get; set; }

		/// <inheritdoc/>
		public DateTimeOffset CreatedAt { get; set; }

		/// <inheritdoc/>
		public DateTimeOffset ModifiedAt { get; set; }

		public DateTimeOffset? CompletedAt { get; set; }

		public string? BankReference { get; set; }

		public string? ExternalReference { get; set; }

		public string? InternalReference { get; set; }

		public string? Description { get; set; }

		public bool Generated { get; set; }

		public bool Validated { get; set; }

		public bool Completed { get; set; }

		public FinanceUser FinanceUser { get; set; }

		public TransactionCategory TransactionCategory { get; set; }

		public Account SourceAccount { get; set; }

		public Account TargetAccount { get; set; }

		public ICollection<TransactionItem> TransactionItems { get; set; }
	}
}
