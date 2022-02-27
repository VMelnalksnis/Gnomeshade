// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.ComponentModel.DataAnnotations;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.WebApi.Models.Tags;

/// <summary>Information needed to create a tag.</summary>
[PublicAPI]
public sealed class TagCreation
{
	/// <summary>The name of the tag.</summary>
	[Required]
	public string Name { get; init; } = null!;

	/// <summary>The description of the tag.</summary>
	public string? Description { get; init; }
}
