// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU General Public License 3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Tracking.Finance.Interfaces.WebApi.v1_0.Authentication
{
	public record LoginResponse(string Token, DateTime ValidTo)
	{
		public string Token { get; init; } = Token;

		public DateTime ValidTo { get; init; } = ValidTo;
	}
}
