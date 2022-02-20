// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;

namespace Gnomeshade.Interfaces.Desktop.Configuration;

/// <summary>
/// Settings for accessing gnomeshade API.
/// </summary>
public sealed record GnomeshadeOptions
{
	internal const string _sectionName = "Gnomeshade";

	/// <summary>
	/// Gets the gnomeshade API base address.
	/// </summary>
	[Required]
	public Uri BaseAddress { get; init; } = null!;
}
