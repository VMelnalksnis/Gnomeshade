using System;
using System.ComponentModel.DataAnnotations;

namespace Tracking.Finance.Interfaces.WebApi.v1_0.Transactions
{
	/// <summary>
	/// Represents information needed in order to create a transaction.
	/// </summary>
	/// <param name="Date">The date on which the transaction was completed on.</param>
	/// <param name="Description"></param>
	/// <param name="Generated"></param>
	/// <param name="Validated"></param>
	/// <param name="Completed"></param>
	public record TransactionCreationModel(
		[Required] DateTimeOffset? Date,
		string? Description,
		bool Generated = true,
		bool Validated = false,
		bool Completed = false);
}
