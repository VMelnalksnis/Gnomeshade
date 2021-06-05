namespace Tracking.Finance.Interfaces.WindowsDesktop.Models
{
	public record LoggedInUserModel
	{
		public string Id { get; set; }

		public string UserName { get; set; }

		public string Email { get; set; }
	}
}
