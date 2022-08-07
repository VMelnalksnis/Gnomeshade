// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Gnomeshade.Data.Identity;

using Microsoft.AspNetCore.Authorization;

namespace Gnomeshade.WebApi.V1_0.Authorization;

/// <summary>An <see cref="IAuthorizationRequirement"/> that indicates that an <see cref="ApplicationUser"/> is required.</summary>
public sealed class ApplicationUserRequirement : IAuthorizationRequirement
{
}
