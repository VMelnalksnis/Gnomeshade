﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using NodaTime;
using NodaTime.Extensions;

namespace Gnomeshade.WebApi.Client;

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

	/// <summary>Raised when a new refresh token has been set.</summary>
	public event EventHandler<RefreshTokenChangedEventArgs>? RefreshTokenChanged;

	/// <summary>Gets the refresh token.</summary>
	public string? Refresh { get; private set; }

	internal string? Access { get; private set; }

	internal bool IsAccessExpired =>
		Access is not null &&
		AccessExpiresAt is not null &&
		_clock.GetCurrentInstant() > AccessExpiresAt;

	private Instant? AccessExpiresAt { get; set; }

	/// <summary>Sets the <see cref="Refresh"/> token.</summary>
	/// <param name="refresh">The refresh token to set.</param>
	public void SetRefreshToken(string refresh)
	{
		Refresh = refresh;
		RefreshTokenChanged?.Invoke(this, new(refresh));
	}

	/// <summary>Clears all stored token data.</summary>
	public void Clear()
	{
		Refresh = null;
		Access = null;
		AccessExpiresAt = null;
	}

	internal void SetRefreshToken(string refresh, string access, DateTimeOffset accessExpires)
	{
		Refresh = refresh;
		RefreshTokenChanged?.Invoke(this, new(refresh));
		Access = access;
		var currentInstant = _clock.GetCurrentInstant();
		AccessExpiresAt = currentInstant + ((accessExpires.ToInstant() - currentInstant) / 2);
	}
}
