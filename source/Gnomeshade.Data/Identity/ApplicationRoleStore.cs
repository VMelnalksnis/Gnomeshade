// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Gnomeshade.Data.Identity;

/// <summary>Persistence store for <see cref="ApplicationRole"/>.</summary>
public sealed class ApplicationRoleStore : RoleStore<ApplicationRole, IdentityContext, Guid>
{
	/// <summary>Initializes a new instance of the <see cref="ApplicationRoleStore"/> class.</summary>
	/// <param name="context">The <see cref="IdentityContext"/>.</param>
	/// <param name="describer">The <see cref="IdentityErrorDescriber"/>.</param>
	public ApplicationRoleStore(IdentityContext context, IdentityErrorDescriber? describer = null)
		: base(context, describer)
	{
	}

	/// <inheritdoc />
	public override Guid ConvertIdFromString(string? id) => id.ConvertIdFromString();

	/// <inheritdoc />
	public override string ConvertIdToString(Guid id) => id.ConvertIdToString();
}
