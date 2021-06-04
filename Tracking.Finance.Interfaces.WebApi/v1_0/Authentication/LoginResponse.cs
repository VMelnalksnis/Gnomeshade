using System;

namespace Tracking.Finance.Interfaces.WebApi.v1_0.Authentication
{
	public record LoginResponse(string Token, DateTime ValidTo)
	{
		public string Token { get; init; } = Token;

		public DateTime ValidTo { get; init; } = ValidTo;
	}
}
