// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using NodaTime;
using NodaTime.Extensions;

namespace Gnomeshade.Interfaces.WebApi.Client;

/// <summary>Caches refresh and access tokens for <see cref="IGnomeshadeClient"/>.</summary>
public sealed class GnomeshadeTokenCache
{
	private readonly IClock _clock;

	/// <summary>Initializes a new instance of the <see cref="GnomeshadeTokenCache"/> class.</summary>
	/// <param name="clock">Clock for accessing the current time.</param>
	public GnomeshadeTokenCache(IClock clock)
	{
		_clock = clock;
	}

	internal string? Access { get; private set; }

	internal bool IsAccessExpired =>
		Access is not null &&
		AccessExpiresAt is not null &&
		_clock.GetCurrentInstant() > AccessExpiresAt;

	internal string? Refresh { get; private set; }

	private Instant? AccessExpiresAt { get; set; }

	internal void SetRefreshToken(string refresh, string access, DateTimeOffset accessExpires)
	{
		Refresh = refresh;
		Access = access;
		var currentInstant = _clock.GetCurrentInstant();
		AccessExpiresAt = currentInstant + ((accessExpires.ToInstant() - currentInstant) / 2);
	}

	internal void SetAccessToken(string accessToken, int accessExpires)
	{
		Access = accessToken;
		var currentInstant = _clock.GetCurrentInstant();
		AccessExpiresAt = currentInstant + Duration.FromSeconds(accessExpires / 2);
	}
}
