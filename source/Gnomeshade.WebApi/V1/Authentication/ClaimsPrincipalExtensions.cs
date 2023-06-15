// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;

using Gnomeshade.WebApi.Configuration;

namespace Gnomeshade.WebApi.V1.Authentication;

internal static class ClaimsPrincipalExtensions
{
	internal static bool TryGetFirstClaimValue(
		this ClaimsPrincipal principal,
		string claimType,
		[NotNullWhen(true)] out string? claimValue)
	{
		var claim = principal.Claims.FirstOrDefault(claim => StringComparer.OrdinalIgnoreCase.Equals(claim.Type, claimType));
		claimValue = claim?.Value;
		return claim is not null;
	}

	internal static Claim? GetSingleOrDefaultClaim(this ClaimsPrincipal principal, string claimType)
	{
		return principal.FindAll(claimType).DistinctBy(claim => claim.Value).SingleOrDefault();
	}

	internal static string? GetLoginProvider(this ClaimsPrincipal principal)
	{
		if (principal.Identity?.AuthenticationType is null)
		{
			return null;
		}

		return principal.Identity.AuthenticationType + AuthConfiguration.OidcSuffix;
	}
}
