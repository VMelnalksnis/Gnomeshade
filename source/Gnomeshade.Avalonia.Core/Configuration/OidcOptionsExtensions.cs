// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using IdentityModel.OidcClient;

namespace Gnomeshade.Avalonia.Core.Configuration;

/// <summary>Helper methods for <see cref="OidcOptions"/>.</summary>
public static class OidcOptionsExtensions
{
	/// <summary>Converts <see cref="OidcOptions"/> to <see cref="OidcClientOptions"/>.</summary>
	/// <param name="options">The options to convert.</param>
	/// <returns>An instance of <see cref="OidcClientOptions"/> with the respective properties from <paramref name="options"/>.</returns>
	public static OidcClientOptions ToOidcClientOptions(this OidcOptions options) => new()
	{
		Authority = options.Authority?.ToString(),
		ClientId = options.ClientId,
		ClientSecret = options.ClientSecret,
		Scope = options.Scope,
		RedirectUri = options.RedirectUri.ToString(),
	};
}
