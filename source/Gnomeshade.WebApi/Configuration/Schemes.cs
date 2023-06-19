// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

namespace Gnomeshade.WebApi.Configuration;

internal static class Schemes
{
	/// <summary>The scheme used to identify application authentication cookies.</summary>
	internal const string Application = $"{_prefix}.Application";

	/// <summary>The scheme used to identify external authentication cookies.</summary>
	internal const string External = $"{_prefix}.External";

	/// <summary>The scheme used to identify Two Factor authentication cookies for saving the Remember Me state.</summary>
	internal const string TwoFactorRememberMe = $"{_prefix}.TwoFactorRememberMe";

	/// <summary>The scheme used to identify Two Factor authentication cookies for round tripping user identities.</summary>
	internal const string TwoFactorUserId = $"{_prefix}.TwoFactorUserId";

	internal const string Bearer = "Bearer";

	private const string _prefix = "Identity";
}
