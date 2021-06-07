namespace Tracking.Finance.Interfaces.WindowsDesktop.Events
{
	/// <summary>
	/// Indicates that a new transaction has been created.
	/// </summary>
	///
	/// <param name="Id">The id of the created transaction.</param>
	public sealed record TransactionCreatedEvent(int Id);
}
