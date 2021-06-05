namespace Tracking.Finance.Interfaces.WebApi.v1_0.Authentication
{
	public record UserModel
	{
		public string Id { get; init; }

		public string UserName { get; init; }

		public string Email { get; init; }
	}
}
