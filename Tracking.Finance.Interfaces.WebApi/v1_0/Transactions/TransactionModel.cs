using System;

namespace Tracking.Finance.Interfaces.WebApi.v1_0.Transactions
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="Id"></param>
	/// <param name="UserId"></param>
	/// <param name="CreatedAt"></param>
	/// <param name="CreatedByUserId"></param>
	/// <param name="ModifiedAt"></param>
	/// <param name="Date"></param>
	/// <param name="Description"></param>
	/// <param name="Generated"></param>
	/// <param name="Validated"></param>
	/// <param name="Completed"></param>
	public record TransactionModel(
		int Id,
		int UserId,
		DateTimeOffset CreatedAt,
		int CreatedByUserId,
		DateTimeOffset ModifiedAt,
		DateTimeOffset Date,
		string? Description,
		bool Generated,
		bool Validated,
		bool Completed);
}
