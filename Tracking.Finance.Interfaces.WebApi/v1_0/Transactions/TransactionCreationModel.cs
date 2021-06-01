using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Tracking.Finance.Interfaces.WebApi.v1_0.Transactions
{
	/// <summary>
	/// Represents information needed in order to create a transaction.
	/// </summary>
	public class TransactionCreationModel
	{
		/// <summary>
		/// The date on which the transaction was completed on.
		/// </summary>
		[Required]
		public DateTimeOffset? Date { get; init; }

		public string? Description { get; init; }

		[DefaultValue(true)]
		public bool Generated { get; init; } = true;

		[DefaultValue(false)]
		public bool Validated { get; init; } = false;

		[DefaultValue(false)]
		public bool Completed { get; init; } = false;
	}
}
