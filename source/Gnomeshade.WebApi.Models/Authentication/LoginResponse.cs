// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using JetBrains.Annotations;

using NodaTime;

namespace Gnomeshade.WebApi.Models.Authentication;

/// <summary>Information about the started session.</summary>
[PublicAPI]
public sealed record LoginResponse(string Token, Instant ValidTo)
{
	/// <summary>A JWT for authenticating the session.</summary>
	public string Token { get; set; } = Token;

	/// <summary>The point in time until which the session is valid.</summary>
	public Instant ValidTo { get; set; } = ValidTo;
}
