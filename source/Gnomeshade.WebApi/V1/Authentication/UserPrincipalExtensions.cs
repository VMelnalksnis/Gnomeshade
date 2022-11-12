// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;

namespace Gnomeshade.WebApi.V1.Authentication;

internal static class UserPrincipalExtensions
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
}
