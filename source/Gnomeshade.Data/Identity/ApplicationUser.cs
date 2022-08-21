// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Microsoft.AspNetCore.Identity;

namespace Gnomeshade.Data.Identity;

/// <summary>Application identity user.</summary>
public sealed class ApplicationUser : IdentityUser
{
	/// <summary>Gets or sets the full name for this user.</summary>
	public string FullName { get; set; } = null!;
}
