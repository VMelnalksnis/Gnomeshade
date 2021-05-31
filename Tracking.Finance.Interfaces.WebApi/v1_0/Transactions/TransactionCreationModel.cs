using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Tracking.Finance.Interfaces.WebApi.v1_0.Transactions
{
	/// <summary>
	/// Represents information needed in order to create a transaction.
	/// </summary>
	public record TransactionCreationModel(
		DateTimeOffset? Date,
		string? Description,
		bool Generated = true,
		bool Validated = false,
		bool Completed = false)
	{
		/// <summary>
		/// The date on which the transaction was completed on.
		/// </summary>
		[Required]
		public DateTimeOffset? Date { get; init; } = Date;

		public string? Description { get; init; } = Description;

		[DefaultValue(true)]
		public bool Generated { get; init; } = Generated;

		[DefaultValue(false)]
		public bool Validated { get; init; } = Validated;

		[DefaultValue(false)]
		public bool Completed { get; init; } = Completed;
	}
}
