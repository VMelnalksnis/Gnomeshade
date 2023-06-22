// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;

using Gnomeshade.Data.Entities;
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

	internal static bool TryGetUserId(this ClaimsPrincipal principal, out Guid id)
	{
		id = Guid.Empty;

		var claims = principal.FindAll(ClaimTypes.NameIdentifier).DistinctBy(claim => claim.Value).ToArray();
		if (claims.Length is not 1)
		{
			return false;
		}

		var claim = claims.Single();
		return Guid.TryParseExact(claim.Value, "D", out id);
	}

	internal static Guid GetUserId(this ClaimsPrincipal principal)
	{
		var claim = principal.FindAll(ClaimTypes.NameIdentifier).DistinctBy(claim => claim.Value).Single();
		return Guid.ParseExact(claim.Value, "D");
	}

	internal static UserEntity ToApplicationUser(this ClaimsPrincipal principal)
	{
		var id = principal.GetUserId();
		return new() { Id = id, CounterpartyId = id };
	}
}
