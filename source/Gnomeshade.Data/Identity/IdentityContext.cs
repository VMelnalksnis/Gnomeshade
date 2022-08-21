// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Gnomeshade.Data.Identity;

/// <summary>Identity database context for <see cref="ApplicationUser"/>.</summary>
public abstract class IdentityContext : IdentityDbContext<ApplicationUser>
{
	/// <summary>The name of the connection string under ConnectionStrings section.</summary>
	protected const string ConnectionStringName = "IdentityDb";
}
