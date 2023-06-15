// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Gnomeshade.WebApi.Client.Results;

/// <summary>User is not registered.</summary>
public sealed class RequiresRegistration : ExternalLoginResult
{
	/// <summary>Initializes a new instance of the <see cref="RequiresRegistration"/> class.</summary>
	/// <param name="redirectUri">The uri the the registration page.</param>
	public RequiresRegistration(Uri redirectUri)
	{
		RedirectUri = redirectUri;
	}

	/// <summary>Gets the uri the the registration page.</summary>
	public Uri RedirectUri { get; }
}
