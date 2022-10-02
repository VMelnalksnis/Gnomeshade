// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Gnomeshade.WebApi.Client;

/// <summary><see cref="EventArgs"/> for the <see cref="GnomeshadeTokenCache.RefreshTokenChanged"/> event.</summary>
public sealed class RefreshTokenChangedEventArgs : EventArgs
{
	/// <summary>Initializes a new instance of the <see cref="RefreshTokenChangedEventArgs"/> class.</summary>
	/// <param name="token">The new refresh token value.</param>
	public RefreshTokenChangedEventArgs(string token)
	{
		Token = token;
	}

	/// <summary>Gets the new refresh token value.</summary>
	public string Token { get; }
}
