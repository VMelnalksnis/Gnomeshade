// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Gnomeshade.WebApi.Configuration;

using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Gnomeshade.WebApi.V1.Authorization;

/// <summary>Requires the user to have the <see cref="Roles.Administrator"/> role.</summary>
public sealed class AdministratorRoleRequirement() : RolesAuthorizationRequirement([Roles.Administrator]);
