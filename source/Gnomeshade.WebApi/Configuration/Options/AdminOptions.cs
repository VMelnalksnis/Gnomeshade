// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.ComponentModel.DataAnnotations;

namespace Gnomeshade.WebApi.Configuration.Options;

/// <summary>Options for configuring the initial admin user.</summary>
public sealed class AdminOptions
{
	internal const string SectionName = "Admin";

	/// <summary>Gets or sets the name of the initial admin user. Defaults to 'Admin'.</summary>
	[Required]
	public string Username { get; set; } = "Admin";

	/// <summary>Gets or sets the password of the initial admin user.</summary>
	[Required]
	public string Password { get; set; } = null!;
}
