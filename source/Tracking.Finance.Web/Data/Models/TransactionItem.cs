using System;

namespace Tracking.Finance.Web.Data.Models
{
	public class TransactionItem : IEntity, IUserSpecificEntity, IModifiableEntity
	{
		/// <inheritdoc/>
		public int Id { get; set; }

		/// <inheritdoc/>
		public int FinanceUserId { get; set; }

		public int TransactionId { get; set; }

		public decimal SourceAmount { get; set; }

		public int SourceAccountId { get; set; }

		public decimal TargetAmount { get; set; }

		public int TargetAccountId { get; set; }

		/// <inheritdoc/>
		public DateTimeOffset CreatedAt { get; set; }

		/// <inheritdoc/>
		public DateTimeOffset ModifiedAt { get; set; }

		public string? BankReference { get; set; }

		public string? ExternalReference { get; set; }

		public string? InternalReference { get; set; }

		public DateTimeOffset? InterestDate { get; set; }

		public DateTimeOffset? BookDate { get; set; }

		public DateTimeOffset? ProcessingDate { get; set; }

		public DateTimeOffset? DueDate { get; set; }

		public DateTimeOffset? PaymentDate { get; set; }

		public DateTimeOffset? InvoiceDate { get; set; }

		public DateTimeOffset? DeliveryDate { get; set; }

		public int ProductId { get; set; }

		public decimal Amount { get; set; }

		public string? Description { get; set; }

		public FinanceUser FinanceUser { get; set; }

		public Transaction Transaction { get; set; }

		public AccountInCurrency SourceAccount { get; set; }

		public AccountInCurrency TargetAccount { get; set; }

		public Product Product { get; set; }
	}
}
