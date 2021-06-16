namespace Tracking.Finance.Web.Models
{
	/// <summary>
	/// Contains information which should be displayed to the user in case of an internal error.
	/// </summary>
	public class ErrorViewModel
	{
		/// <summary>
		/// Gets or sets the id of the request during which an error occured.
		/// </summary>
		public string? RequestId { get; set; }

		/// <summary>
		/// Gets a value indicating whether request id should be showed.
		/// </summary>
		public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
	}
}
